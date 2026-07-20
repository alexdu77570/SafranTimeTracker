import { zodResolver } from '@hookform/resolvers/zod'
import Autocomplete from '@mui/material/Autocomplete'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import { useMutation, useQuery } from '@tanstack/react-query'
import { Controller, useForm, type Control, type FieldPath, type FieldValues } from 'react-hook-form'
import { z } from 'zod'
import { fetchApplications } from '../../../../api/endpoints/applications'
import { fetchResources } from '../../../../api/endpoints/resources'
import { createTechnology, updateTechnology } from '../../../../api/endpoints/technologies'
import type { TechnologyDto } from '../../../../api/types'
import { ReferentialStatus } from '../../../../api/types'
import { FormField } from '../../../../components/ui/FormField'
import { FormSelect } from '../../../../components/ui/FormSelect'

interface MultiSelectOption {
  id: string
  label: string
}

/** Multi-sélection reliée à React Hook Form (Applications/Ressources rattachées à une technologie,
 * docs/BACKLOG_METIER.md §5) — un besoin ponctuel, pas généralisé dans FormSelect (qui ne gère que
 * la sélection simple, CLAUDE.md §5 : pas d'abstraction anticipée pour un seul formulaire). */
function MultiSelectField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  options,
}: {
  control: Control<TFieldValues>
  name: FieldPath<TFieldValues>
  label: string
  options: MultiSelectOption[]
}) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field }) => (
        <Autocomplete
          multiple
          size="small"
          options={options}
          getOptionLabel={(option) => option.label}
          isOptionEqualToValue={(option, value) => option.id === value.id}
          value={options.filter((option) => (field.value as string[] | undefined)?.includes(option.id))}
          onChange={(_, selected) => field.onChange(selected.map((option) => option.id))}
          renderInput={(params) => <TextField {...params} label={label} />}
        />
      )}
    />
  )
}

function useApplicationResourceOptions() {
  const applications = useQuery({ queryKey: ['applications', 'all'], queryFn: () => fetchApplications({ pageSize: 100 }) })
  const resources = useQuery({ queryKey: ['resources', 'all'], queryFn: () => fetchResources({ pageSize: 100 }) })
  return {
    applicationOptions: applications.data?.items.map((a) => ({ id: a.id, label: a.nom })) ?? [],
    resourceOptions: resources.data?.items.map((r) => ({ id: r.id, label: `${r.prenom} ${r.nom}` })) ?? [],
  }
}

const createSchema = z.object({
  code: z.string().min(1, 'Code obligatoire').max(30),
  libelle: z.string().min(1, 'Libellé obligatoire').max(100),
  applicationIds: z.array(z.string()),
  resourceIds: z.array(z.string()),
})
type CreateFormValues = z.infer<typeof createSchema>

export function TechnologyCreateForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const { applicationOptions, resourceOptions } = useApplicationResourceOptions()
  const { control, handleSubmit } = useForm<CreateFormValues>({
    resolver: zodResolver(createSchema),
    defaultValues: { code: '', libelle: '', applicationIds: [], resourceIds: [] },
  })
  const mutation = useMutation({ mutationFn: createTechnology, onSuccess })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="code" label="Code" />
        <FormField control={control} name="libelle" label="Libellé" />
        <MultiSelectField control={control} name="applicationIds" label="Applications" options={applicationOptions} />
        <MultiSelectField control={control} name="resourceIds" label="Ressources" options={resourceOptions} />
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
  applicationIds: z.array(z.string()),
  resourceIds: z.array(z.string()),
})
type UpdateFormValues = z.infer<typeof updateSchema>

export function TechnologyEditForm({
  row,
  onSuccess,
  onCancel,
}: {
  row: TechnologyDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { applicationOptions, resourceOptions } = useApplicationResourceOptions()
  const { control, handleSubmit } = useForm<UpdateFormValues>({
    resolver: zodResolver(updateSchema),
    defaultValues: {
      libelle: row.libelle,
      statut: String(row.statut),
      applicationIds: [...row.applicationIds],
      resourceIds: [...row.resourceIds],
    },
  })
  const mutation = useMutation({
    mutationFn: (values: UpdateFormValues) =>
      updateTechnology(row.id, { ...values, statut: Number(values.statut) as ReferentialStatus }),
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
        <MultiSelectField control={control} name="applicationIds" label="Applications" options={applicationOptions} />
        <MultiSelectField control={control} name="resourceIds" label="Ressources" options={resourceOptions} />
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
