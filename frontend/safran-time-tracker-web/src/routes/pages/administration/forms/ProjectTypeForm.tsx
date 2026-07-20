import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { createProjectType, updateProjectType } from '../../../../api/endpoints/projectTypes'
import type { ProjectTypeDto } from '../../../../api/types'
import { ReferentialStatus } from '../../../../api/types'
import { FormField } from '../../../../components/ui/FormField'
import { FormSelect } from '../../../../components/ui/FormSelect'

const createSchema = z.object({
  code: z.string().min(1, 'Code obligatoire').max(30),
  libelle: z.string().min(1, 'Libellé obligatoire').max(100),
})
type CreateFormValues = z.infer<typeof createSchema>

export function ProjectTypeCreateForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const { control, handleSubmit } = useForm<CreateFormValues>({
    resolver: zodResolver(createSchema),
    defaultValues: { code: '', libelle: '' },
  })
  const mutation = useMutation({ mutationFn: createProjectType, onSuccess })

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

const updateSchema = z.object({
  libelle: z.string().min(1, 'Libellé obligatoire').max(100),
  statut: z.string(),
})
type UpdateFormValues = z.infer<typeof updateSchema>

export function ProjectTypeEditForm({
  row,
  onSuccess,
  onCancel,
}: {
  row: ProjectTypeDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { control, handleSubmit } = useForm<UpdateFormValues>({
    resolver: zodResolver(updateSchema),
    defaultValues: { libelle: row.libelle, statut: String(row.statut) },
  })
  const mutation = useMutation({
    mutationFn: (values: UpdateFormValues) =>
      updateProjectType(row.id, { libelle: values.libelle, statut: Number(values.statut) as ReferentialStatus }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="libelle" label="Libellé" />
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
