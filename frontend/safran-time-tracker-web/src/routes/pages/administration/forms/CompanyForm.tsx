import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { createCompany, updateCompany } from '../../../../api/endpoints/companies'
import type { CompanyDto } from '../../../../api/types'
import { FormField } from '../../../../components/ui/FormField'
import { FormSelect } from '../../../../components/ui/FormSelect'
import { COMPANY_TYPE_OPTIONS } from '../../../../lib/knownReferentials'

const createSchema = z.object({
  code: z.string().min(1, 'Code obligatoire').max(20),
  nom: z.string().min(1, 'Nom obligatoire').max(200),
  companyTypeId: z.string().min(1, 'Type obligatoire'),
  contactPrincipal: z.string().min(1, 'Contact principal obligatoire').max(200),
  emailContact: z.email('Email invalide').max(200),
  telephone: z.string().max(30).optional(),
  adresse: z.string().max(500).optional(),
  commentaire: z.string().max(1000).optional(),
})
type CreateFormValues = z.infer<typeof createSchema>

export function CompanyCreateForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const { control, handleSubmit } = useForm<CreateFormValues>({
    resolver: zodResolver(createSchema),
    defaultValues: {
      code: '', nom: '', companyTypeId: '', contactPrincipal: '', emailContact: '', telephone: '', adresse: '', commentaire: '',
    },
  })
  const mutation = useMutation({ mutationFn: createCompany, onSuccess })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="code" label="Code" />
        <FormField control={control} name="nom" label="Nom" />
        <FormSelect control={control} name="companyTypeId" label="Type" options={COMPANY_TYPE_OPTIONS} />
        <FormField control={control} name="contactPrincipal" label="Contact principal" />
        <FormField control={control} name="emailContact" label="Email de contact" type="email" />
        <FormField control={control} name="telephone" label="Téléphone (optionnel)" />
        <FormField control={control} name="adresse" label="Adresse (optionnel)" />
        <FormField control={control} name="commentaire" label="Commentaire (optionnel)" multiline rows={2} />
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
  companyTypeId: z.string().min(1, 'Type obligatoire'),
  contactPrincipal: z.string().min(1, 'Contact principal obligatoire').max(200),
  emailContact: z.email('Email invalide').max(200),
  telephone: z.string().max(30).optional(),
  adresse: z.string().max(500).optional(),
  commentaire: z.string().max(1000).optional(),
})
type UpdateFormValues = z.infer<typeof updateSchema>

export function CompanyEditForm({
  row,
  onSuccess,
  onCancel,
}: {
  row: CompanyDto
  onSuccess: () => void
  onCancel: () => void
}) {
  const { control, handleSubmit } = useForm<UpdateFormValues>({
    resolver: zodResolver(updateSchema),
    defaultValues: {
      nom: row.nom,
      companyTypeId: row.companyTypeId,
      contactPrincipal: row.contactPrincipal,
      emailContact: row.emailContact,
      telephone: row.telephone ?? '',
      adresse: row.adresse ?? '',
      commentaire: row.commentaire ?? '',
    },
  })
  const mutation = useMutation({
    mutationFn: (values: UpdateFormValues) => updateCompany(row.id, values),
    onSuccess,
  })

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
      <Stack spacing={2}>
        <FormField control={control} name="nom" label="Nom" />
        <FormSelect control={control} name="companyTypeId" label="Type" options={COMPANY_TYPE_OPTIONS} />
        <FormField control={control} name="contactPrincipal" label="Contact principal" />
        <FormField control={control} name="emailContact" label="Email de contact" type="email" />
        <FormField control={control} name="telephone" label="Téléphone (optionnel)" />
        <FormField control={control} name="adresse" label="Adresse (optionnel)" />
        <FormField control={control} name="commentaire" label="Commentaire (optionnel)" multiline rows={2} />
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
