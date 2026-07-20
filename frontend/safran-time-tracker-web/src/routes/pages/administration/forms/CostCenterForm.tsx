import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { fetchDepartments } from '../../../../api/endpoints/organisation'
import { createCostCenter, updateCostCenter } from '../../../../api/endpoints/costCenters'
import { fetchServices } from '../../../../api/endpoints/organisation'
import type { CostCenterDto } from '../../../../api/types'
import { ReferentialStatus } from '../../../../api/types'
import { FormField } from '../../../../components/ui/FormField'
import { FormSelect } from '../../../../components/ui/FormSelect'

function useDepartmentServiceOptions() {
  const departments = useQuery({ queryKey: ['departments', 'all'], queryFn: () => fetchDepartments() })
  const services = useQuery({ queryKey: ['services', 'all'], queryFn: () => fetchServices() })
  return {
    departmentOptions: [
      { value: '', label: '(aucun)' },
      ...(departments.data?.items.map((d) => ({ value: d.id, label: d.nom })) ?? []),
    ],
    serviceOptions: [
      { value: '', label: '(aucun)' },
      ...(services.data?.items.map((s) => ({ value: s.id, label: s.nom })) ?? []),
    ],
  }
}

const createSchema = z.object({
  code: z.string().min(1, 'Code obligatoire').max(30),
  libelle: z.string().min(1, 'Libellé obligatoire').max(100),
  departmentId: z.string().optional(),
  serviceId: z.string().optional(),
})
type CreateFormValues = z.infer<typeof createSchema>

export function CostCenterCreateForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const { departmentOptions, serviceOptions } = useDepartmentServiceOptions()
  const { control, handleSubmit } = useForm<CreateFormValues>({
    resolver: zodResolver(createSchema),
    defaultValues: { code: '', libelle: '', departmentId: '', serviceId: '' },
  })
  const mutation = useMutation({
    mutationFn: (values: CreateFormValues) =>
      createCostCenter({
        code: values.code,
        libelle: values.libelle,
        departmentId: values.departmentId || null,
        serviceId: values.serviceId || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="code" label="Code" />
        <FormField control={control} name="libelle" label="Libellé" />
        <FormSelect control={control} name="departmentId" label="Département" options={departmentOptions} />
        <FormSelect control={control} name="serviceId" label="Service" options={serviceOptions} />
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
  departmentId: z.string().optional(),
  serviceId: z.string().optional(),
  statut: z.string(),
})
type UpdateFormValues = z.infer<typeof updateSchema>

export function CostCenterEditForm({
  row,
  onSuccess,
  onCancel,
}: {
  row: CostCenterDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { departmentOptions, serviceOptions } = useDepartmentServiceOptions()
  const { control, handleSubmit } = useForm<UpdateFormValues>({
    resolver: zodResolver(updateSchema),
    defaultValues: {
      libelle: row.libelle,
      departmentId: row.departmentId ?? '',
      serviceId: row.serviceId ?? '',
      statut: String(row.statut),
    },
  })
  const mutation = useMutation({
    mutationFn: (values: UpdateFormValues) =>
      updateCostCenter(row.id, {
        libelle: values.libelle,
        departmentId: values.departmentId || null,
        serviceId: values.serviceId || null,
        statut: Number(values.statut) as ReferentialStatus,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="libelle" label="Libellé" />
        <FormSelect control={control} name="departmentId" label="Département" options={departmentOptions} />
        <FormSelect control={control} name="serviceId" label="Service" options={serviceOptions} />
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
