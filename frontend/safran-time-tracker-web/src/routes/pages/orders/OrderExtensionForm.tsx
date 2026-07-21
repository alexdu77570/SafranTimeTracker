import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Chip from '@mui/material/Chip'
import Stack from '@mui/material/Stack'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { createOrderExtension } from '../../../api/endpoints/orderExtensions'
import { FormField } from '../../../components/ui/FormField'

const schema = z.object({
  amountAdded: z.coerce.number().positive('Doit être positif'),
  daysAdded: z.coerce.number().nonnegative().optional(),
  newEndDate: z.string().min(1, 'Nouvelle date de fin obligatoire'),
  reason: z.string().min(1, 'Motif obligatoire'),
  comment: z.string().optional(),
})
type FormValues = z.infer<typeof schema>

/** Rallonge de commande (§13.3) : augmente le budget ajusté, conserve le budget initial, motif
 * obligatoire — ExtensionDate/PreviousEndDate sont dérivés côté serveur, jamais saisis ici. */
export function OrderExtensionCreateForm({
  orderId,
  onSuccess,
  onCancel,
}: {
  orderId: string
  onSuccess: () => void
  onCancel: () => void
}) {
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      amountAdded: 0,
      daysAdded: undefined,
      newEndDate: '',
      reason: '',
      comment: '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      createOrderExtension(orderId, {
        amountAdded: values.amountAdded,
        daysAdded: values.daysAdded ?? null,
        newEndDate: values.newEndDate,
        reason: values.reason,
        comment: values.comment || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <Chip label="Motif obligatoire (§13.3)" size="small" sx={{ alignSelf: 'flex-start' }} />
        <FormField control={control} name="amountAdded" label="Montant ajouté (€)" type="number" />
        <FormField
          control={control}
          name="daysAdded"
          label="Jours ajoutés (facultatif)"
          type="number"
        />
        <FormField control={control} name="newEndDate" label="Nouvelle date de fin" type="date" />
        <FormField control={control} name="reason" label="Motif" multiline rows={2} />
        <FormField
          control={control}
          name="comment"
          label="Commentaire (facultatif)"
          multiline
          rows={2}
        />
        <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
          <Button onClick={onCancel}>Annuler</Button>
          <Button type="submit" variant="contained" loading={mutation.isPending}>
            Créer la rallonge
          </Button>
        </Stack>
      </Stack>
    </form>
  )
}
