import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { createOrderReceipt } from '../../../api/endpoints/orderReceipts'
import { FormField } from '../../../components/ui/FormField'
import { PermissionCodes } from '../../../auth/permissionCodes'
import { useCurrentUser } from '../../../auth/useCurrentUser'

const schema = z
  .object({
    receiptDate: z.string().min(1, 'Date de réception obligatoire'),
    receivedAmount: z.coerce.number().optional(),
    receivedDays: z.coerce.number().optional(),
    reason: z.string().optional(),
    comment: z.string().optional(),
  })
  .refine((values) => (values.receivedAmount ?? 0) !== 0 || (values.receivedDays ?? 0) !== 0, {
    message: 'Renseigner un montant ou un nombre de jours reçus',
    path: ['receivedAmount'],
  })
  .refine((values) => !((values.receivedAmount ?? 0) !== 0 && (values.receivedDays ?? 0) !== 0), {
    message: 'Une réception porte sur un montant OU des jours, jamais les deux',
    path: ['receivedDays'],
  })
type FormValues = z.infer<typeof schema>

/** Réception partielle (règle métier validée Lot 6) : append-only, une valeur négative est une
 * écriture compensatoire explicite. Le dépassement du reste réceptionnable n'est jamais un champ du
 * formulaire : le serveur l'autorise silencieusement si l'appelant porte ORDER_RECEIPT_OVERRIDE,
 * sinon renvoie un conflit 409 (docs/BACKLOG_METIER.md §1). */
export function OrderReceiptCreateForm({
  orderId,
  onSuccess,
  onCancel,
}: {
  orderId: string
  onSuccess: () => void
  onCancel: () => void
}) {
  const { hasPermission } = useCurrentUser()
  const { control, handleSubmit } = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      receiptDate: '',
      receivedAmount: undefined,
      receivedDays: undefined,
      reason: '',
      comment: '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      createOrderReceipt(orderId, {
        receiptDate: values.receiptDate,
        receivedAmount: values.receivedAmount || null,
        receivedDays: values.receivedDays || null,
        reason: values.reason || null,
        comment: values.comment || null,
      }),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="receiptDate" label="Date de réception" type="date" />
        <FormField
          control={control}
          name="receivedAmount"
          label="Montant reçu (€, ou jours)"
          type="number"
        />
        <FormField
          control={control}
          name="receivedDays"
          label="Jours reçus (ou montant)"
          type="number"
        />
        <FormField control={control} name="reason" label="Motif (facultatif)" />
        <FormField
          control={control}
          name="comment"
          label="Commentaire (facultatif)"
          multiline
          rows={2}
        />
        {!hasPermission(PermissionCodes.OrderReceiptOverride) && (
          <Typography variant="caption" color="text.secondary">
            Un dépassement du reste réceptionnable sera refusé sans la permission dédiée.
          </Typography>
        )}
        <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
          <Button onClick={onCancel}>Annuler</Button>
          <Button type="submit" variant="contained" loading={mutation.isPending}>
            Enregistrer la réception
          </Button>
        </Stack>
      </Stack>
    </form>
  )
}
