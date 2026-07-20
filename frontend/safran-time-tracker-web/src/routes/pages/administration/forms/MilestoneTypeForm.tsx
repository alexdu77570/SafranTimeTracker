import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { createMilestoneType } from '../../../../api/endpoints/milestoneTypes'
import { FormField } from '../../../../components/ui/FormField'

const schema = z.object({
  code: z.string().min(1, 'Code obligatoire').max(30),
  libelle: z.string().min(1, 'Libellé obligatoire').max(100),
})
type FormValues = z.infer<typeof schema>

export function MilestoneTypeCreateForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const { control, handleSubmit } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', libelle: '' },
  })
  const mutation = useMutation({ mutationFn: createMilestoneType, onSuccess })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="code" label="Code" />
        <FormField control={control} name="libelle" label="Libellé" />
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
