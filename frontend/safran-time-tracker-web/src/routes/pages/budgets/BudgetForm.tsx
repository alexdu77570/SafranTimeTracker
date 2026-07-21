import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { adjustBudget, createBudget, updateBudget } from '../../../api/endpoints/budgets'
import { fetchOrders } from '../../../api/endpoints/orders'
import { fetchProjects } from '../../../api/endpoints/projects'
import type { BudgetDto } from '../../../api/types'
import { FormField } from '../../../components/ui/FormField'
import { FormSelect } from '../../../components/ui/FormSelect'

function useFormOptions() {
  const projects = useQuery({
    queryKey: ['projects', 'all'],
    queryFn: () => fetchProjects({ pageSize: 100 }),
  })
  const orders = useQuery({
    queryKey: ['orders', 'all'],
    queryFn: () => fetchOrders({ pageSize: 100 }),
  })

  return {
    projectOptions: [
      { value: '', label: '(aucun)' },
      ...(projects.data?.items.map((p) => ({ value: p.id, label: p.nom })) ?? []),
    ],
    orderOptions: [
      { value: '', label: '(aucune)' },
      ...(orders.data?.items.map((o) => ({ value: o.id, label: o.reference })) ?? []),
    ],
  }
}

const createSchema = z.object({
  name: z.string().min(1, 'Nom obligatoire'),
  projectId: z.string().optional(),
  orderId: z.string().optional(),
  initialAmount: z.coerce.number().nonnegative('Doit être positif'),
  alertThreshold: z.coerce.number().nonnegative().optional(),
  startDate: z.string().min(1, 'Date de début obligatoire'),
  endDate: z.string().optional(),
  comment: z.string().optional(),
})
type CreateFormValues = z.infer<typeof createSchema>

/** §14.1 : un budget est lié à un projet et/ou une commande (au moins l'un des deux, simplification
 * documentée) — les deux restent facultatifs au niveau du formulaire, la règle est vérifiée côté
 * serveur. */
export function BudgetCreateForm({
  onSuccess,
  onCancel,
}: {
  onSuccess: () => void
  onCancel: () => void
}) {
  const { projectOptions, orderOptions } = useFormOptions()
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(createSchema),
    defaultValues: {
      name: '',
      projectId: '',
      orderId: '',
      initialAmount: 0,
      alertThreshold: undefined,
      startDate: '',
      endDate: '',
      comment: '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: CreateFormValues) =>
      createBudget({
        name: values.name,
        projectId: values.projectId || null,
        orderId: values.orderId || null,
        initialAmount: values.initialAmount,
        alertThreshold: values.alertThreshold ?? null,
        startDate: values.startDate,
        endDate: values.endDate || null,
        comment: values.comment || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="name" label="Nom" />
        <FormSelect
          control={control}
          name="projectId"
          label="Projet (facultatif)"
          options={projectOptions}
        />
        <FormSelect
          control={control}
          name="orderId"
          label="Commande (facultative)"
          options={orderOptions}
        />
        <FormField
          control={control}
          name="initialAmount"
          label="Montant initial (€)"
          type="number"
        />
        <FormField
          control={control}
          name="alertThreshold"
          label="Seuil d'alerte (facultatif)"
          type="number"
        />
        <FormField control={control} name="startDate" label="Date de début" type="date" />
        <FormField control={control} name="endDate" label="Date de fin (facultative)" type="date" />
        <FormField control={control} name="comment" label="Commentaire" multiline rows={2} />
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
  name: z.string().min(1, 'Nom obligatoire'),
  alertThreshold: z.coerce.number().nonnegative().optional(),
  endDate: z.string().optional(),
  comment: z.string().optional(),
})
type UpdateFormValues = z.infer<typeof updateSchema>

export function BudgetEditForm({
  budget,
  onSuccess,
  onCancel,
}: {
  budget: BudgetDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(updateSchema),
    defaultValues: {
      name: budget.name,
      alertThreshold: budget.alertThreshold ?? undefined,
      endDate: budget.endDate ?? '',
      comment: budget.comment ?? '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: UpdateFormValues) =>
      updateBudget(budget.id, {
        name: values.name,
        alertThreshold: values.alertThreshold ?? null,
        endDate: values.endDate || null,
        comment: values.comment || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="name" label="Nom" />
        <FormField
          control={control}
          name="alertThreshold"
          label="Seuil d'alerte (facultatif)"
          type="number"
        />
        <FormField control={control} name="endDate" label="Date de fin (facultative)" type="date" />
        <FormField control={control} name="comment" label="Commentaire" multiline rows={2} />
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

const adjustSchema = z.object({
  newValue: z.coerce.number().nonnegative('Doit être positif'),
  reason: z.string().min(1, 'Motif obligatoire'),
  referencePiece: z.string().optional(),
})
type AdjustFormValues = z.infer<typeof adjustSchema>

/** §14.2 : chaque ajustement conserve ancienne valeur, nouvelle valeur, motif, auteur, date —
 * OldValue est dérivé côté serveur (AdjustedAmount courant), seul NewValue est saisi ici. */
export function BudgetAdjustForm({
  budget,
  onSuccess,
  onCancel,
}: {
  budget: BudgetDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(adjustSchema),
    defaultValues: { newValue: budget.adjustedAmount, reason: '', referencePiece: '' },
  })
  const mutation = useMutation({
    mutationFn: (values: AdjustFormValues) =>
      adjustBudget(budget.id, {
        newValue: values.newValue,
        reason: values.reason,
        referencePiece: values.referencePiece || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField
          control={control}
          name="newValue"
          label="Nouveau montant ajusté (€)"
          type="number"
        />
        <FormField control={control} name="reason" label="Motif" multiline rows={2} />
        <FormField
          control={control}
          name="referencePiece"
          label="Pièce de référence (facultative, texte seul)"
        />
        <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
          <Button onClick={onCancel}>Annuler</Button>
          <Button type="submit" variant="contained" loading={mutation.isPending}>
            Ajuster
          </Button>
        </Stack>
      </Stack>
    </form>
  )
}
