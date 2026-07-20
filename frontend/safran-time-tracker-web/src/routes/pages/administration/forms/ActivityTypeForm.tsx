import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Checkbox from '@mui/material/Checkbox'
import FormControlLabel from '@mui/material/FormControlLabel'
import Stack from '@mui/material/Stack'
import { useMutation } from '@tanstack/react-query'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import { createActivityType } from '../../../../api/endpoints/activityTypes'
import { FormField } from '../../../../components/ui/FormField'

const schema = z.object({
  code: z.string().min(1, 'Code obligatoire').max(30),
  libelle: z.string().min(1, 'Libellé obligatoire').max(100),
  isRun: z.boolean(),
  referenceRequired: z.boolean(),
  referenceFormatRegex: z.string().optional(),
  referenceExample: z.string().optional(),
})
type FormValues = z.infer<typeof schema>

export function ActivityTypeCreateForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const { control, handleSubmit } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', libelle: '', isRun: false, referenceRequired: false, referenceFormatRegex: '', referenceExample: '' },
  })
  const mutation = useMutation({ mutationFn: createActivityType, onSuccess })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="code" label="Code" />
        <FormField control={control} name="libelle" label="Libellé" />
        <Controller
          control={control}
          name="isRun"
          render={({ field }) => (
            <FormControlLabel
              control={<Checkbox checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />}
              label="Activité RUN"
            />
          )}
        />
        <Controller
          control={control}
          name="referenceRequired"
          render={({ field }) => (
            <FormControlLabel
              control={<Checkbox checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />}
              label="Référence obligatoire à la saisie"
            />
          )}
        />
        <FormField control={control} name="referenceFormatRegex" label="Format de référence (regex, optionnel)" />
        <FormField control={control} name="referenceExample" label="Exemple de référence (optionnel)" />
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
