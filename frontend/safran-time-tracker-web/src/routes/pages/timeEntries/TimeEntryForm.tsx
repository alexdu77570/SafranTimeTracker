import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { fetchActivityTypes } from '../../../api/endpoints/activityTypes'
import { fetchOrders } from '../../../api/endpoints/orders'
import { fetchProjects } from '../../../api/endpoints/projects'
import { createTimeEntry, updateTimeEntry } from '../../../api/endpoints/timeEntries'
import type { ActivityTypeDto, TimeEntryDto } from '../../../api/types'
import { FormDatePicker } from '../../../components/ui/FormDatePicker'
import { FormField } from '../../../components/ui/FormField'
import { FormSelect } from '../../../components/ui/FormSelect'

const schema = z.object({
  activityTypeId: z.string().min(1, "Type d'activité obligatoire"),
  date: z.string().min(1, 'Date obligatoire'),
  dureeHeures: z.coerce
    .number()
    .positive('La durée doit être strictement positive')
    .max(24, 'La durée ne peut excéder 24 heures'),
  projectId: z.string().optional(),
  orderId: z.string().optional(),
  reference: z.string().optional(),
  commentaire: z.string().max(1000).optional(),
})
type FormValues = z.infer<typeof schema>

/** Validation de référence pilotée par les métadonnées d'ActivityType (§19.3), rejouée
 * côté serveur de toute façon (CLAUDE.md §7) — pas une seconde règle métier, la même donnée
 * (ReferenceRequired/ReferenceFormatRegex/ReferenceExample) est simplement lue côté client. */
export function validateReference(
  activityType: ActivityTypeDto | undefined,
  reference: string | undefined,
): string | null {
  if (!activityType) {
    return null
  }
  if (activityType.referenceRequired && !reference?.trim()) {
    return `La référence est obligatoire pour le type '${activityType.libelle}' (exemple : ${activityType.referenceExample ?? '—'}).`
  }
  if (
    reference?.trim() &&
    activityType.referenceFormatRegex &&
    !new RegExp(activityType.referenceFormatRegex).test(reference)
  ) {
    return `Format de référence invalide pour '${activityType.libelle}' (exemple : ${activityType.referenceExample ?? '—'}).`
  }
  return null
}

function useFormOptions() {
  const activityTypes = useQuery({
    queryKey: ['activity-types', 'all'],
    queryFn: () => fetchActivityTypes({ pageSize: 100 }),
  })
  const projects = useQuery({ queryKey: ['projects', 'all'], queryFn: () => fetchProjects() })
  const orders = useQuery({
    queryKey: ['orders', 'all'],
    queryFn: () => fetchOrders({ pageSize: 100 }),
  })

  return {
    activityTypeOptions:
      activityTypes.data?.items.map((a) => ({ value: a.id, label: a.libelle })) ?? [],
    activityTypesById: new Map((activityTypes.data?.items ?? []).map((a) => [a.id, a])),
    projectOptions: [
      { value: '', label: '(aucun)' },
      ...(projects.data?.items.map((p) => ({ value: p.id, label: p.nom })) ?? []),
    ],
    orderOptions: [
      { value: '', label: '(aucune)' },
      ...(orders.data?.items.map((o) => ({ value: o.id, label: o.reference })) ?? []),
    ],
  }
}

export function TimeEntryCreateForm({
  resourceId,
  seed,
  onSuccess,
  onCancel,
}: {
  resourceId: string
  seed?: Partial<FormValues>
  onSuccess: () => void
  onCancel: () => void
}) {
  const { activityTypeOptions, activityTypesById, projectOptions, orderOptions } = useFormOptions()
  const { control, handleSubmit, setError } = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      activityTypeId: '',
      date: '',
      dureeHeures: 7.75,
      projectId: '',
      orderId: '',
      reference: '',
      commentaire: '',
      ...seed,
    },
  })
  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      createTimeEntry({
        resourceId,
        activityTypeId: values.activityTypeId,
        date: values.date,
        dureeHeures: values.dureeHeures,
        projectId: values.projectId || null,
        orderId: values.orderId || null,
        reference: values.reference || null,
        commentaire: values.commentaire || null,
      }),
    onSuccess,
  })

  const onSubmit = (values: FormValues) => {
    const referenceError = validateReference(
      activityTypesById.get(values.activityTypeId),
      values.reference,
    )
    if (referenceError) {
      setError('reference', { message: referenceError })
      return
    }
    mutation.mutate(values)
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Stack spacing={2}>
        <FormSelect
          control={control}
          name="activityTypeId"
          label="Type d'activité"
          options={activityTypeOptions}
        />
        <FormDatePicker control={control} name="date" label="Date" />
        <FormField control={control} name="dureeHeures" label="Durée (heures)" type="number" />
        <FormSelect
          control={control}
          name="projectId"
          label="Projet (facultatif)"
          options={projectOptions}
        />
        <FormSelect
          control={control}
          name="orderId"
          label="Commande (facultative)"
          options={orderOptions}
        />
        <FormField control={control} name="reference" label="Référence" />
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

export function TimeEntryEditForm({
  row,
  onSuccess,
  onCancel,
}: {
  row: TimeEntryDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { activityTypeOptions, activityTypesById, projectOptions, orderOptions } = useFormOptions()
  const { control, handleSubmit, setError } = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      activityTypeId: row.activityTypeId,
      date: row.date,
      dureeHeures: row.dureeHeures,
      projectId: row.projectId ?? '',
      orderId: row.orderId ?? '',
      reference: row.reference ?? '',
      commentaire: row.commentaire ?? '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      updateTimeEntry(row.id, {
        activityTypeId: values.activityTypeId,
        date: values.date,
        dureeHeures: values.dureeHeures,
        projectId: values.projectId || null,
        orderId: values.orderId || null,
        reference: values.reference || null,
        commentaire: values.commentaire || null,
      }),
    onSuccess,
  })

  const onSubmit = (values: FormValues) => {
    const referenceError = validateReference(
      activityTypesById.get(values.activityTypeId),
      values.reference,
    )
    if (referenceError) {
      setError('reference', { message: referenceError })
      return
    }
    mutation.mutate(values)
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Stack spacing={2}>
        <FormSelect
          control={control}
          name="activityTypeId"
          label="Type d'activité"
          options={activityTypeOptions}
        />
        <FormDatePicker control={control} name="date" label="Date" />
        <FormField control={control} name="dureeHeures" label="Durée (heures)" type="number" />
        <FormSelect
          control={control}
          name="projectId"
          label="Projet (facultatif)"
          options={projectOptions}
        />
        <FormSelect
          control={control}
          name="orderId"
          label="Commande (facultative)"
          options={orderOptions}
        />
        <FormField control={control} name="reference" label="Référence" />
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
