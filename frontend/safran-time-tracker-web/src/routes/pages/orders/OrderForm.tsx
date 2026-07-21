import { zodResolver } from '@hookform/resolvers/zod'
import Autocomplete from '@mui/material/Autocomplete'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import { useMutation, useQuery } from '@tanstack/react-query'
import {
  Controller,
  useForm,
  type Control,
  type FieldPath,
  type FieldValues,
} from 'react-hook-form'
import { z } from 'zod'
import { fetchCompanies } from '../../../api/endpoints/companies'
import { createOrder, updateOrder } from '../../../api/endpoints/orders'
import { fetchProjects } from '../../../api/endpoints/projects'
import { fetchResources } from '../../../api/endpoints/resources'
import type { OrderDto } from '../../../api/types'
import { FormField } from '../../../components/ui/FormField'
import { FormSelect } from '../../../components/ui/FormSelect'

interface MultiSelectOption {
  id: string
  label: string
}

/** Même besoin ponctuel que TechnologyForm.tsx (Lot 8, §5 "pas d'abstraction anticipée") : la
 * sélection multiple des ressources autorisées (§13.2) n'est pas généralisée dans FormSelect. */
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
          value={options.filter((option) =>
            (field.value as string[] | undefined)?.includes(option.id),
          )}
          onChange={(_, selected) => field.onChange(selected.map((option) => option.id))}
          renderInput={(params) => <TextField {...params} label={label} />}
        />
      )}
    />
  )
}

function useFormOptions() {
  const companies = useQuery({
    queryKey: ['companies', 'all'],
    queryFn: () => fetchCompanies({ pageSize: 100 }),
  })
  const projects = useQuery({
    queryKey: ['projects', 'all'],
    queryFn: () => fetchProjects({ pageSize: 100 }),
  })
  const resources = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })

  return {
    companyOptions: companies.data?.items.map((c) => ({ value: c.id, label: c.nom })) ?? [],
    projectOptions: [
      { value: '', label: '(aucun)' },
      ...(projects.data?.items.map((p) => ({ value: p.id, label: p.nom })) ?? []),
    ],
    resourceOptions:
      resources.data?.items.map((r) => ({ id: r.id, label: `${r.prenom} ${r.nom}` })) ?? [],
  }
}

const createSchema = z.object({
  reference: z.string().min(1, 'Référence obligatoire'),
  libelle: z.string().min(1, 'Libellé obligatoire'),
  companyId: z.string().min(1, 'Société obligatoire'),
  projectId: z.string().optional(),
  budgetFinancierInitial: z.coerce.number().nonnegative('Doit être positif'),
  budgetJoursInitial: z.coerce.number().nonnegative().optional(),
  dateDebut: z.string().min(1, 'Date de début obligatoire'),
  dateFinInitiale: z.string().min(1, 'Date de fin initiale obligatoire'),
  seuilAlerte: z.coerce.number().nonnegative().optional(),
  commentaire: z.string().optional(),
  authorizedResourceIds: z.array(z.string()),
})
type CreateFormValues = z.infer<typeof createSchema>

export function OrderCreateForm({
  onSuccess,
  onCancel,
}: {
  onSuccess: () => void
  onCancel: () => void
}) {
  const { companyOptions, projectOptions, resourceOptions } = useFormOptions()
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(createSchema),
    defaultValues: {
      reference: '',
      libelle: '',
      companyId: '',
      projectId: '',
      budgetFinancierInitial: 0,
      budgetJoursInitial: undefined,
      dateDebut: '',
      dateFinInitiale: '',
      seuilAlerte: undefined,
      commentaire: '',
      authorizedResourceIds: [] as string[],
    },
  })
  const mutation = useMutation({
    mutationFn: (values: CreateFormValues) =>
      createOrder({
        reference: values.reference,
        libelle: values.libelle,
        companyId: values.companyId,
        projectId: values.projectId || null,
        budgetFinancierInitial: values.budgetFinancierInitial,
        budgetJoursInitial: values.budgetJoursInitial ?? null,
        dateDebut: values.dateDebut,
        dateFinInitiale: values.dateFinInitiale,
        seuilAlerte: values.seuilAlerte ?? null,
        commentaire: values.commentaire || null,
        authorizedResourceIds: values.authorizedResourceIds,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="reference" label="Référence" />
        <FormField control={control} name="libelle" label="Libellé" />
        <FormSelect control={control} name="companyId" label="Société" options={companyOptions} />
        <FormSelect
          control={control}
          name="projectId"
          label="Projet (facultatif)"
          options={projectOptions}
        />
        <FormField
          control={control}
          name="budgetFinancierInitial"
          label="Budget financier initial (€)"
          type="number"
        />
        <FormField
          control={control}
          name="budgetJoursInitial"
          label="Budget en jours initial (facultatif)"
          type="number"
        />
        <FormField control={control} name="dateDebut" label="Date de début" type="date" />
        <FormField
          control={control}
          name="dateFinInitiale"
          label="Date de fin initiale"
          type="date"
        />
        <FormField
          control={control}
          name="seuilAlerte"
          label="Seuil d'alerte (facultatif)"
          type="number"
        />
        <MultiSelectField
          control={control}
          name="authorizedResourceIds"
          label="Ressources autorisées"
          options={resourceOptions}
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
  libelle: z.string().min(1, 'Libellé obligatoire'),
  projectId: z.string().optional(),
  seuilAlerte: z.coerce.number().nonnegative().optional(),
  commentaire: z.string().optional(),
  authorizedResourceIds: z.array(z.string()),
})
type UpdateFormValues = z.infer<typeof updateSchema>

export function OrderEditForm({
  order,
  onSuccess,
  onCancel,
}: {
  order: OrderDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { projectOptions, resourceOptions } = useFormOptions()
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(updateSchema),
    defaultValues: {
      libelle: order.libelle,
      projectId: order.projectId ?? '',
      seuilAlerte: order.seuilAlerte ?? undefined,
      commentaire: order.commentaire ?? '',
      authorizedResourceIds: order.authorizedResourceIds,
    },
  })
  const mutation = useMutation({
    mutationFn: (values: UpdateFormValues) =>
      updateOrder(order.id, {
        libelle: values.libelle,
        projectId: values.projectId || null,
        seuilAlerte: values.seuilAlerte ?? null,
        commentaire: values.commentaire || null,
        authorizedResourceIds: values.authorizedResourceIds,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="libelle" label="Libellé" />
        <FormSelect
          control={control}
          name="projectId"
          label="Projet (facultatif)"
          options={projectOptions}
        />
        <FormField
          control={control}
          name="seuilAlerte"
          label="Seuil d'alerte (facultatif)"
          type="number"
        />
        <MultiSelectField
          control={control}
          name="authorizedResourceIds"
          label="Ressources autorisées"
          options={resourceOptions}
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
