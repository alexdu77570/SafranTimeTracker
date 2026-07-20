import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { createClient, updateClient } from '../../../../api/endpoints/clients'
import type { ClientDto } from '../../../../api/types'
import { ReferentialStatus } from '../../../../api/types'
import { FormField } from '../../../../components/ui/FormField'
import { FormSelect } from '../../../../components/ui/FormSelect'

const createSchema = z.object({
  code: z.string().min(1, 'Code obligatoire').max(30),
  nom: z.string().min(1, 'Nom obligatoire').max(200),
  commentaire: z.string().max(1000).optional(),
})
type CreateFormValues = z.infer<typeof createSchema>

export function ClientCreateForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const { control, handleSubmit } = useForm<CreateFormValues>({
    resolver: zodResolver(createSchema),
    defaultValues: { code: '', nom: '', commentaire: '' },
  })
  const mutation = useMutation({ mutationFn: createClient, onSuccess })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="code" label="Code" />
        <FormField control={control} name="nom" label="Nom" />
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
  nom: z.string().min(1, 'Nom obligatoire').max(200),
  // FormSelect (TextField select) émet des chaînes ; conversion vers ReferentialStatus au submit.
  statut: z.string(),
  commentaire: z.string().max(1000).optional(),
})
type UpdateFormValues = z.infer<typeof updateSchema>

export function ClientEditForm({
  row,
  onSuccess,
  onCancel,
}: {
  row: ClientDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { control, handleSubmit } = useForm<UpdateFormValues>({
    resolver: zodResolver(updateSchema),
    defaultValues: { nom: row.nom, statut: String(row.statut), commentaire: row.commentaire ?? '' },
  })
  const mutation = useMutation({
    mutationFn: (values: UpdateFormValues) =>
      updateClient(row.id, { ...values, statut: Number(values.statut) as ReferentialStatus }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="nom" label="Nom" />
        <FormSelect
          control={control}
          name="statut"
          label="Statut"
          options={[
            { value: String(ReferentialStatus.Actif), label: 'Actif' },
            { value: String(ReferentialStatus.Inactif), label: 'Inactif' },
          ]}
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
