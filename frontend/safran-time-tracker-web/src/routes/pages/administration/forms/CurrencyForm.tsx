import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { createCurrency, updateCurrency } from '../../../../api/endpoints/currencies'
import type { CurrencyDto } from '../../../../api/types'
import { ReferentialStatus } from '../../../../api/types'
import { FormField } from '../../../../components/ui/FormField'
import { FormSelect } from '../../../../components/ui/FormSelect'

const createSchema = z.object({
  codeIso: z
    .string()
    .length(3, 'Le code ISO doit contenir exactement 3 lettres')
    .regex(/^[A-Z]{3}$/, 'Lettres majuscules uniquement (ISO 4217)'),
  libelle: z.string().min(1, 'Libellé obligatoire').max(100),
  symbole: z.string().min(1, 'Symbole obligatoire').max(5),
})
type CreateFormValues = z.infer<typeof createSchema>

export function CurrencyCreateForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const { control, handleSubmit } = useForm<CreateFormValues>({
    resolver: zodResolver(createSchema),
    defaultValues: { codeIso: '', libelle: '', symbole: '' },
  })
  const mutation = useMutation({ mutationFn: createCurrency, onSuccess })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="codeIso" label="Code ISO 4217 (ex. EUR)" />
        <FormField control={control} name="libelle" label="Libellé" />
        <FormField control={control} name="symbole" label="Symbole" />
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
  libelle: z.string().min(1, 'Libellé obligatoire').max(100),
  symbole: z.string().min(1, 'Symbole obligatoire').max(5),
  statut: z.string(),
})
type UpdateFormValues = z.infer<typeof updateSchema>

export function CurrencyEditForm({
  row,
  onSuccess,
  onCancel,
}: {
  row: CurrencyDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { control, handleSubmit } = useForm<UpdateFormValues>({
    resolver: zodResolver(updateSchema),
    defaultValues: { libelle: row.libelle, symbole: row.symbole, statut: String(row.statut) },
  })
  const mutation = useMutation({
    mutationFn: (values: UpdateFormValues) =>
      updateCurrency(row.id, {
        libelle: values.libelle,
        symbole: values.symbole,
        statut: Number(values.statut) as ReferentialStatus,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="libelle" label="Libellé" />
        <FormField control={control} name="symbole" label="Symbole" />
        <FormSelect
          control={control}
          name="statut"
          label="Statut"
          options={[
            { value: String(ReferentialStatus.Actif), label: 'Actif' },
            { value: String(ReferentialStatus.Inactif), label: 'Inactif' },
          ]}
        />
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
