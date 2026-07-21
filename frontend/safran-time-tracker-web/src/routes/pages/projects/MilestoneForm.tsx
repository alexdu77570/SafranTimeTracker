import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { fetchApplications } from '../../../api/endpoints/applications'
import {
  createMilestone,
  fetchMilestones,
  updateMilestone,
} from '../../../api/endpoints/milestones'
import { fetchMilestoneTypes } from '../../../api/endpoints/milestoneTypes'
import { fetchResources } from '../../../api/endpoints/resources'
import type { MilestoneDto } from '../../../api/types'
import { MilestoneCriticality, MilestoneStatus } from '../../../api/types'
import { FormField } from '../../../components/ui/FormField'
import { FormSelect } from '../../../components/ui/FormSelect'

const criticiteOptions = [
  { value: String(MilestoneCriticality.Faible), label: 'Faible' },
  { value: String(MilestoneCriticality.Moyenne), label: 'Moyenne' },
  { value: String(MilestoneCriticality.Elevee), label: 'Élevée' },
  { value: String(MilestoneCriticality.Critique), label: 'Critique' },
]

const statutOptions = [
  { value: String(MilestoneStatus.AVenir), label: 'À venir' },
  { value: String(MilestoneStatus.EnCours), label: 'En cours' },
  { value: String(MilestoneStatus.Termine), label: 'Terminé' },
  { value: String(MilestoneStatus.Annule), label: 'Annulé' },
]

function useFormOptions(projectId: string, excludeMilestoneId?: string) {
  const types = useQuery({
    queryKey: ['milestone-types', 'all'],
    queryFn: () => fetchMilestoneTypes(),
  })
  const applications = useQuery({
    queryKey: ['applications', 'all'],
    queryFn: () => fetchApplications(),
  })
  const resources = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const otherMilestones = useQuery({
    queryKey: ['milestones', 'byProject', projectId],
    queryFn: () => fetchMilestones({ projectId, pageSize: 100 }),
  })

  return {
    typeOptions: types.data?.items.map((t) => ({ value: t.id, label: t.libelle })) ?? [],
    applicationOptions: [
      { value: '', label: '(aucune)' },
      ...(applications.data?.items.map((a) => ({ value: a.id, label: a.nom })) ?? []),
    ],
    responsableOptions:
      resources.data?.items.map((r) => ({ value: r.id, label: `${r.prenom} ${r.nom}` })) ?? [],
    dependsOnOptions: [
      { value: '', label: '(aucune)' },
      ...(otherMilestones.data?.items
        .filter((m) => m.id !== excludeMilestoneId)
        .map((m) => ({ value: m.id, label: m.nom })) ?? []),
    ],
  }
}

const createSchema = z.object({
  nom: z.string().min(1, 'Nom obligatoire'),
  milestoneTypeId: z.string().min(1, 'Type obligatoire'),
  applicationId: z.string().optional(),
  responsableId: z.string().min(1, 'Responsable obligatoire'),
  datePrevue: z.string().min(1, 'Date prévue obligatoire'),
  criticite: z.string(),
  commentaire: z.string().optional(),
  dependsOnMilestoneId: z.string().optional(),
})
type CreateFormValues = z.infer<typeof createSchema>

export function MilestoneCreateForm({
  projectId,
  onSuccess,
  onCancel,
}: {
  projectId: string
  onSuccess: () => void
  onCancel: () => void
}) {
  const { typeOptions, applicationOptions, responsableOptions, dependsOnOptions } =
    useFormOptions(projectId)
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(createSchema),
    defaultValues: {
      nom: '',
      milestoneTypeId: '',
      applicationId: '',
      responsableId: '',
      datePrevue: '',
      criticite: String(MilestoneCriticality.Moyenne),
      commentaire: '',
      dependsOnMilestoneId: '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: CreateFormValues) =>
      createMilestone({
        nom: values.nom,
        milestoneTypeId: values.milestoneTypeId,
        projectId,
        applicationId: values.applicationId || null,
        responsableId: values.responsableId,
        datePrevue: values.datePrevue,
        criticite: Number(values.criticite) as MilestoneCriticality,
        commentaire: values.commentaire || null,
        dependsOnMilestoneId: values.dependsOnMilestoneId || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="nom" label="Nom" />
        <FormSelect
          control={control}
          name="milestoneTypeId"
          label="Type de jalon"
          options={typeOptions}
        />
        <FormSelect
          control={control}
          name="applicationId"
          label="Application (facultative)"
          options={applicationOptions}
        />
        <FormSelect
          control={control}
          name="responsableId"
          label="Responsable"
          options={responsableOptions}
        />
        <FormField control={control} name="datePrevue" label="Date prévue" type="date" />
        <FormSelect
          control={control}
          name="criticite"
          label="Criticité"
          options={criticiteOptions}
        />
        <FormSelect
          control={control}
          name="dependsOnMilestoneId"
          label="Dépend du jalon (facultatif)"
          options={dependsOnOptions}
        />
        <FormField control={control} name="commentaire" label="Commentaire" multiline rows={2} />
        <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
          <Button onClick={onCancel}>Annuler</Button>
          <Button type="submit" variant="contained" loading={mutation.isPending}>
            Créer
          </Button>
        </Stack>
      </Stack>
    </form>
  )
}

const updateSchema = z.object({
  nom: z.string().min(1, 'Nom obligatoire'),
  responsableId: z.string().min(1, 'Responsable obligatoire'),
  datePrevue: z.string().min(1, 'Date prévue obligatoire'),
  dateReelle: z.string().optional(),
  statut: z.string(),
  criticite: z.string(),
  commentaire: z.string().optional(),
  dependsOnMilestoneId: z.string().optional(),
})
type UpdateFormValues = z.infer<typeof updateSchema>

export function MilestoneEditForm({
  milestone,
  onSuccess,
  onCancel,
}: {
  milestone: MilestoneDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { responsableOptions, dependsOnOptions } = useFormOptions(milestone.projectId, milestone.id)
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(updateSchema),
    defaultValues: {
      nom: milestone.nom,
      responsableId: milestone.responsableId,
      datePrevue: milestone.datePrevue,
      dateReelle: milestone.dateReelle ?? '',
      statut: String(milestone.statut),
      criticite: String(milestone.criticite),
      commentaire: milestone.commentaire ?? '',
      dependsOnMilestoneId: milestone.dependsOnMilestoneId ?? '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: UpdateFormValues) =>
      updateMilestone(milestone.id, {
        nom: values.nom,
        responsableId: values.responsableId,
        datePrevue: values.datePrevue,
        dateReelle: values.dateReelle || null,
        statut: Number(values.statut) as MilestoneStatus,
        criticite: Number(values.criticite) as MilestoneCriticality,
        commentaire: values.commentaire || null,
        dependsOnMilestoneId: values.dependsOnMilestoneId || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="nom" label="Nom" />
        <FormSelect
          control={control}
          name="responsableId"
          label="Responsable"
          options={responsableOptions}
        />
        <FormField control={control} name="datePrevue" label="Date prévue" type="date" />
        <FormField
          control={control}
          name="dateReelle"
          label="Date réelle (facultative)"
          type="date"
        />
        <FormSelect control={control} name="statut" label="Statut" options={statutOptions} />
        <FormSelect
          control={control}
          name="criticite"
          label="Criticité"
          options={criticiteOptions}
        />
        <FormSelect
          control={control}
          name="dependsOnMilestoneId"
          label="Dépend du jalon (facultatif)"
          options={dependsOnOptions}
        />
        <FormField control={control} name="commentaire" label="Commentaire" multiline rows={2} />
        <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
          <Button onClick={onCancel}>Annuler</Button>
          <Button type="submit" variant="contained" loading={mutation.isPending}>
            Enregistrer
          </Button>
        </Stack>
      </Stack>
    </form>
  )
}
