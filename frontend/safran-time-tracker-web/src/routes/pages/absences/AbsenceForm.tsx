import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Checkbox from '@mui/material/Checkbox'
import FormControlLabel from '@mui/material/FormControlLabel'
import Stack from '@mui/material/Stack'
import { useMutation } from '@tanstack/react-query'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import { createAbsence, updateAbsence } from '../../../api/endpoints/absences'
import type { AbsenceDto } from '../../../api/types'
import { AbsenceType } from '../../../api/types'
import { FormDatePicker } from '../../../components/ui/FormDatePicker'
import { FormField } from '../../../components/ui/FormField'
import { FormSelect } from '../../../components/ui/FormSelect'

const typeOptions = [
  { value: String(AbsenceType.Conge), label: 'Congé' },
  { value: String(AbsenceType.Rtt), label: 'RTT' },
  { value: String(AbsenceType.Maladie), label: 'Maladie' },
  { value: String(AbsenceType.Formation), label: 'Formation' },
  { value: String(AbsenceType.Deplacement), label: 'Déplacement' },
  { value: String(AbsenceType.Indisponible), label: 'Indisponible' },
]

const schema = z.object({
  type: z.string().min(1, 'Type obligatoire'),
  dateDebut: z.string().min(1, 'Date de début obligatoire'),
  dateFin: z.string().min(1, 'Date de fin obligatoire'),
  demiJournee: z.boolean(),
  commentaire: z.string().max(1000).optional(),
})
type FormValues = z.infer<typeof schema>

export function AbsenceCreateForm({
  resourceId,
  onSuccess,
  onCancel,
}: {
  resourceId: string
  onSuccess: () => void
  onCancel: () => void
}) {
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      type: String(AbsenceType.Conge),
      dateDebut: '',
      dateFin: '',
      demiJournee: false,
      commentaire: '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      createAbsence({
        resourceId,
        type: Number(values.type) as AbsenceType,
        dateDebut: values.dateDebut,
        dateFin: values.dateFin,
        demiJournee: values.demiJournee,
        commentaire: values.commentaire || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormSelect control={control} name="type" label="Type d'absence" options={typeOptions} />
        <FormDatePicker control={control} name="dateDebut" label="Date de début" />
        <FormDatePicker control={control} name="dateFin" label="Date de fin" />
        <Controller
          control={control}
          name="demiJournee"
          render={({ field }) => (
            <FormControlLabel
              control={
                <Checkbox
                  checked={field.value}
                  onChange={(e) => field.onChange(e.target.checked)}
                />
              }
              label="Demi-journée"
            />
          )}
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

/** Restreint au statut Brouillon côté serveur (409 sinon, docs/BACKLOG_METIER.md §12) — le
 * formulaire n'est proposé que sur une ligne au statut Brouillon (voir AbsencesPage). */
export function AbsenceEditForm({
  row,
  onSuccess,
  onCancel,
}: {
  row: AbsenceDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      type: String(row.type),
      dateDebut: row.dateDebut,
      dateFin: row.dateFin,
      demiJournee: row.demiJournee,
      commentaire: row.commentaire ?? '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      updateAbsence(row.id, {
        type: Number(values.type) as AbsenceType,
        dateDebut: values.dateDebut,
        dateFin: values.dateFin,
        demiJournee: values.demiJournee,
        commentaire: values.commentaire || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormSelect control={control} name="type" label="Type d'absence" options={typeOptions} />
        <FormDatePicker control={control} name="dateDebut" label="Date de début" />
        <FormDatePicker control={control} name="dateFin" label="Date de fin" />
        <Controller
          control={control}
          name="demiJournee"
          render={({ field }) => (
            <FormControlLabel
              control={
                <Checkbox
                  checked={field.value}
                  onChange={(e) => field.onChange(e.target.checked)}
                />
              }
              label="Demi-journée"
            />
          )}
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
