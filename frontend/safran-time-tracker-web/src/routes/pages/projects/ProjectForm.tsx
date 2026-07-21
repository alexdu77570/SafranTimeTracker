import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { fetchApplications } from '../../../api/endpoints/applications'
import { fetchClients } from '../../../api/endpoints/clients'
import { fetchDepartments, fetchServices, fetchTeams } from '../../../api/endpoints/organisation'
import { createProject, updateProject } from '../../../api/endpoints/projects'
import { fetchProjectTypes } from '../../../api/endpoints/projectTypes'
import { fetchResources } from '../../../api/endpoints/resources'
import type { ProjectDto } from '../../../api/types'
import { ProjectRiskLevel } from '../../../api/types'
import { FormField } from '../../../components/ui/FormField'
import { FormSelect } from '../../../components/ui/FormSelect'

const riskLevelOptions = [
  { value: String(ProjectRiskLevel.Faible), label: 'Faible' },
  { value: String(ProjectRiskLevel.Moyen), label: 'Moyen' },
  { value: String(ProjectRiskLevel.Eleve), label: 'Élevé' },
]

const createSchema = z.object({
  nom: z.string().min(1, 'Nom obligatoire'),
  code: z.string().min(1, 'Code obligatoire'),
  applicationId: z.string().min(1, 'Application obligatoire'),
  descriptionCourte: z.string().optional(),
  piloteId: z.string().min(1, 'Pilote obligatoire'),
  departmentId: z.string().min(1, 'Département obligatoire'),
  serviceId: z.string().min(1, 'Service obligatoire'),
  teamId: z.string().optional(),
  projectTypeId: z.string().optional(),
  clientId: z.string().optional(),
  dateDebut: z.string().min(1, 'Date de début obligatoire'),
  dateFinPrevueInitiale: z.string().min(1, 'Date de fin prévue obligatoire'),
  budgetInitial: z.coerce.number().nonnegative().optional(),
  niveauRisque: z.string(),
  commentaire: z.string().optional(),
})
type CreateFormValues = z.infer<typeof createSchema>

const updateSchema = z.object({
  nom: z.string().min(1, 'Nom obligatoire'),
  descriptionCourte: z.string().optional(),
  piloteId: z.string().min(1, 'Pilote obligatoire'),
  teamId: z.string().optional(),
  projectTypeId: z.string().optional(),
  clientId: z.string().optional(),
  dateFinAjustee: z.string().min(1, 'Date de fin ajustée obligatoire'),
  dateFinReelle: z.string().optional(),
  budgetInitial: z.coerce.number().nonnegative().optional(),
  niveauRisque: z.string(),
  commentaire: z.string().optional(),
})
type UpdateFormValues = z.infer<typeof updateSchema>

function useFormOptions() {
  const applications = useQuery({
    queryKey: ['applications', 'all'],
    queryFn: () => fetchApplications(),
  })
  const resources = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const departments = useQuery({
    queryKey: ['departments', 'all'],
    queryFn: () => fetchDepartments(),
  })
  const services = useQuery({ queryKey: ['services', 'all'], queryFn: () => fetchServices() })
  const teams = useQuery({ queryKey: ['teams', 'all'], queryFn: () => fetchTeams() })
  const projectTypes = useQuery({
    queryKey: ['project-types', 'all'],
    queryFn: () => fetchProjectTypes(),
  })
  const clients = useQuery({ queryKey: ['clients', 'all'], queryFn: () => fetchClients() })

  return {
    applicationOptions: applications.data?.items.map((a) => ({ value: a.id, label: a.nom })) ?? [],
    piloteOptions:
      resources.data?.items.map((r) => ({ value: r.id, label: `${r.prenom} ${r.nom}` })) ?? [],
    departmentOptions: departments.data?.items.map((d) => ({ value: d.id, label: d.nom })) ?? [],
    serviceOptions: services.data?.items.map((s) => ({ value: s.id, label: s.nom })) ?? [],
    teamOptions: [
      { value: '', label: '(aucune)' },
      ...(teams.data?.items.map((t) => ({ value: t.id, label: t.nom })) ?? []),
    ],
    projectTypeOptions: [
      { value: '', label: '(aucun)' },
      ...(projectTypes.data?.items.map((t) => ({ value: t.id, label: t.libelle })) ?? []),
    ],
    clientOptions: [
      { value: '', label: '(aucun)' },
      ...(clients.data?.items.map((c) => ({ value: c.id, label: c.nom })) ?? []),
    ],
  }
}

export function ProjectCreateForm({
  onSuccess,
  onCancel,
}: {
  onSuccess: () => void
  onCancel: () => void
}) {
  const {
    applicationOptions,
    piloteOptions,
    departmentOptions,
    serviceOptions,
    teamOptions,
    projectTypeOptions,
    clientOptions,
  } = useFormOptions()
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(createSchema),
    defaultValues: {
      nom: '',
      code: '',
      applicationId: '',
      descriptionCourte: '',
      piloteId: '',
      departmentId: '',
      serviceId: '',
      teamId: '',
      projectTypeId: '',
      clientId: '',
      dateDebut: '',
      dateFinPrevueInitiale: '',
      budgetInitial: undefined,
      niveauRisque: String(ProjectRiskLevel.Faible),
      commentaire: '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: CreateFormValues) =>
      createProject({
        nom: values.nom,
        code: values.code,
        applicationId: values.applicationId,
        descriptionCourte: values.descriptionCourte || null,
        piloteId: values.piloteId,
        departmentId: values.departmentId,
        serviceId: values.serviceId,
        teamId: values.teamId || null,
        projectTypeId: values.projectTypeId || null,
        clientId: values.clientId || null,
        dateDebut: values.dateDebut,
        dateFinPrevueInitiale: values.dateFinPrevueInitiale,
        budgetInitial: values.budgetInitial ?? null,
        niveauRisque: Number(values.niveauRisque) as ProjectRiskLevel,
        commentaire: values.commentaire || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="nom" label="Nom" />
        <FormField control={control} name="code" label="Code" />
        <FormSelect
          control={control}
          name="applicationId"
          label="Application"
          options={applicationOptions}
        />
        <FormField control={control} name="descriptionCourte" label="Description courte" />
        <FormSelect control={control} name="piloteId" label="Pilote" options={piloteOptions} />
        <FormSelect
          control={control}
          name="departmentId"
          label="Département"
          options={departmentOptions}
        />
        <FormSelect control={control} name="serviceId" label="Service" options={serviceOptions} />
        <FormSelect
          control={control}
          name="teamId"
          label="Équipe (facultative)"
          options={teamOptions}
        />
        <FormSelect
          control={control}
          name="projectTypeId"
          label="Type de projet (facultatif)"
          options={projectTypeOptions}
        />
        <FormSelect
          control={control}
          name="clientId"
          label="Client (facultatif)"
          options={clientOptions}
        />
        <FormField control={control} name="dateDebut" label="Date de début" type="date" />
        <FormField
          control={control}
          name="dateFinPrevueInitiale"
          label="Date de fin prévue initiale"
          type="date"
        />
        <FormField
          control={control}
          name="budgetInitial"
          label="Budget initial (€)"
          type="number"
        />
        <FormSelect
          control={control}
          name="niveauRisque"
          label="Niveau de risque"
          options={riskLevelOptions}
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

export function ProjectEditForm({
  project,
  onSuccess,
  onCancel,
}: {
  project: ProjectDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { piloteOptions, teamOptions, projectTypeOptions, clientOptions } = useFormOptions()
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(updateSchema),
    defaultValues: {
      nom: project.nom,
      descriptionCourte: project.descriptionCourte ?? '',
      piloteId: project.piloteId,
      teamId: project.teamId ?? '',
      projectTypeId: project.projectTypeId ?? '',
      clientId: project.clientId ?? '',
      dateFinAjustee: project.dateFinAjustee ?? project.dateFinPrevueInitiale,
      dateFinReelle: project.dateFinReelle ?? '',
      budgetInitial: project.financialSummary?.budgetInitial ?? undefined,
      niveauRisque: String(project.niveauRisque),
      commentaire: project.commentaire ?? '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: UpdateFormValues) =>
      updateProject(project.id, {
        nom: values.nom,
        descriptionCourte: values.descriptionCourte || null,
        piloteId: values.piloteId,
        teamId: values.teamId || null,
        projectTypeId: values.projectTypeId || null,
        clientId: values.clientId || null,
        dateFinAjustee: values.dateFinAjustee,
        dateFinReelle: values.dateFinReelle || null,
        budgetInitial: values.budgetInitial ?? null,
        niveauRisque: Number(values.niveauRisque) as ProjectRiskLevel,
        commentaire: values.commentaire || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="nom" label="Nom" />
        <FormField control={control} name="descriptionCourte" label="Description courte" />
        <FormSelect control={control} name="piloteId" label="Pilote" options={piloteOptions} />
        <FormSelect
          control={control}
          name="teamId"
          label="Équipe (facultative)"
          options={teamOptions}
        />
        <FormSelect
          control={control}
          name="projectTypeId"
          label="Type de projet (facultatif)"
          options={projectTypeOptions}
        />
        <FormSelect
          control={control}
          name="clientId"
          label="Client (facultatif)"
          options={clientOptions}
        />
        <FormField
          control={control}
          name="dateFinAjustee"
          label="Date de fin ajustée"
          type="date"
        />
        <FormField control={control} name="dateFinReelle" label="Date de fin réelle" type="date" />
        <FormField
          control={control}
          name="budgetInitial"
          label="Budget initial (€)"
          type="number"
        />
        <FormSelect
          control={control}
          name="niveauRisque"
          label="Niveau de risque"
          options={riskLevelOptions}
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
