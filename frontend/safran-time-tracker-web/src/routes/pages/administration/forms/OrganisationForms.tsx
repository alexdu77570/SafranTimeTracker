import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { createDepartment, createService, createTeam, fetchDepartments, fetchServices } from '../../../../api/endpoints/organisation'
import { FormField } from '../../../../components/ui/FormField'
import { FormSelect } from '../../../../components/ui/FormSelect'

const departmentSchema = z.object({
  code: z.string().min(1, 'Code obligatoire').max(30),
  nom: z.string().min(1, 'Nom obligatoire').max(200),
})
type DepartmentFormValues = z.infer<typeof departmentSchema>

export function DepartmentCreateForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const { control, handleSubmit } = useForm<DepartmentFormValues>({
    resolver: zodResolver(departmentSchema),
    defaultValues: { code: '', nom: '' },
  })
  const mutation = useMutation({ mutationFn: createDepartment, onSuccess })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="code" label="Code" />
        <FormField control={control} name="nom" label="Nom" />
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

const serviceSchema = z.object({
  code: z.string().min(1, 'Code obligatoire').max(30),
  nom: z.string().min(1, 'Nom obligatoire').max(200),
  departmentId: z.string().min(1, 'Département obligatoire'),
})
type ServiceFormValues = z.infer<typeof serviceSchema>

export function ServiceCreateForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const departments = useQuery({ queryKey: ['departments', 'all'], queryFn: () => fetchDepartments() })
  const { control, handleSubmit } = useForm<ServiceFormValues>({
    resolver: zodResolver(serviceSchema),
    defaultValues: { code: '', nom: '', departmentId: '' },
  })
  const mutation = useMutation({ mutationFn: createService, onSuccess })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="code" label="Code" />
        <FormField control={control} name="nom" label="Nom" />
        <FormSelect
          control={control}
          name="departmentId"
          label="Département"
          options={departments.data?.items.map((d) => ({ value: d.id, label: d.nom })) ?? []}
        />
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

const teamSchema = z.object({
  code: z.string().min(1, 'Code obligatoire').max(30),
  nom: z.string().min(1, 'Nom obligatoire').max(200),
  serviceId: z.string().min(1, 'Service obligatoire'),
})
type TeamFormValues = z.infer<typeof teamSchema>

export function TeamCreateForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const services = useQuery({ queryKey: ['services', 'all'], queryFn: () => fetchServices() })
  const { control, handleSubmit } = useForm<TeamFormValues>({
    resolver: zodResolver(teamSchema),
    defaultValues: { code: '', nom: '', serviceId: '' },
  })
  const mutation = useMutation({ mutationFn: createTeam, onSuccess })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="code" label="Code" />
        <FormField control={control} name="nom" label="Nom" />
        <FormSelect
          control={control}
          name="serviceId"
          label="Service"
          options={services.data?.items.map((s) => ({ value: s.id, label: s.nom })) ?? []}
        />
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
