import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { fetchOrders } from '../../../api/endpoints/orders'
import { createProjectParticipant } from '../../../api/endpoints/projectParticipants'
import { fetchResources } from '../../../api/endpoints/resources'
import { FormField } from '../../../components/ui/FormField'
import { FormSelect } from '../../../components/ui/FormSelect'
import { OPERATIONAL_ROLE_OPTIONS } from '../../../lib/knownReferentials'

const schema = z.object({
  resourceId: z.string().min(1, 'Ressource obligatoire'),
  operationalRoleId: z.string().optional(),
  defaultOrderId: z.string().optional(),
  dateDebut: z.string().min(1, 'Date de début obligatoire'),
  dateFin: z.string().optional(),
  capacitePrevue: z.coerce.number().nonnegative().optional(),
})
type FormValues = z.infer<typeof schema>

/** Création d'un participant (§17.2). Pas de formulaire de modification : le backend n'expose que
 * Création et Retrait (désactivation) — décision actée à l'ouverture du Lot 10. */
export function ProjectParticipantCreateForm({
  projectId,
  onSuccess,
  onCancel,
}: {
  projectId: string
  onSuccess: () => void
  onCancel: () => void
}) {
  const resourcesQuery = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const ordersQuery = useQuery({
    queryKey: ['orders', 'all'],
    queryFn: () => fetchOrders({ pageSize: 100 }),
  })

  const resourceOptions =
    resourcesQuery.data?.items.map((r) => ({ value: r.id, label: `${r.prenom} ${r.nom}` })) ?? []
  const roleOptions = [{ value: '', label: '(aucun)' }, ...OPERATIONAL_ROLE_OPTIONS]
  const orderOptions = [
    { value: '', label: '(aucune)' },
    ...(ordersQuery.data?.items.map((o) => ({ value: o.id, label: o.reference })) ?? []),
  ]

  const { control, handleSubmit } = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      resourceId: '',
      operationalRoleId: '',
      defaultOrderId: '',
      dateDebut: '',
      dateFin: '',
      capacitePrevue: undefined,
    },
  })
  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      createProjectParticipant(projectId, {
        resourceId: values.resourceId,
        operationalRoleId: values.operationalRoleId || null,
        defaultOrderId: values.defaultOrderId || null,
        dateDebut: values.dateDebut,
        dateFin: values.dateFin || null,
        capacitePrevue: values.capacitePrevue ?? null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormSelect
          control={control}
          name="resourceId"
          label="Ressource"
          options={resourceOptions}
        />
        <FormSelect
          control={control}
          name="operationalRoleId"
          label="Rôle opérationnel (facultatif)"
          options={roleOptions}
        />
        <FormSelect
          control={control}
          name="defaultOrderId"
          label="Commande par défaut (facultative)"
          options={orderOptions}
        />
        <FormField control={control} name="dateDebut" label="Date de début" type="date" />
        <FormField control={control} name="dateFin" label="Date de fin (facultative)" type="date" />
        <FormField
          control={control}
          name="capacitePrevue"
          label="Capacité prévue (jours)"
          type="number"
        />
        <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
          <Button onClick={onCancel}>Annuler</Button>
          <Button type="submit" variant="contained" loading={mutation.isPending}>
            Ajouter
          </Button>
        </Stack>
      </Stack>
    </form>
  )
}
