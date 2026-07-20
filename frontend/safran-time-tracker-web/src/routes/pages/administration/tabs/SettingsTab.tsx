import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Checkbox from '@mui/material/Checkbox'
import FormControlLabel from '@mui/material/FormControlLabel'
import Grid from '@mui/material/Grid'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Controller, useForm } from 'react-hook-form'
import { useEffect } from 'react'
import { z } from 'zod'
import { fetchSettings, updateSettings } from '../../../../api/endpoints/settings'
import { EmptyState } from '../../../../components/ui/EmptyState'
import { FormField } from '../../../../components/ui/FormField'

// SettingsDto porte des seuils `number | null` (jamais absents) : un champ vidé dans le formulaire
// (chaîne vide) est ramené à `null`, jamais coercé en 0, pour ne pas changer le sens du paramètre.
const nullableNumber = z.preprocess(
  (value) => (value === '' || value === null || value === undefined ? null : value),
  z.coerce.number().nullable(),
)

const schema = z.object({
  heuresParJour: z.coerce.number().positive(),
  joursOuvresParSemaine: z.coerce.number().int().positive(),
  paysParDefaut: z.string().min(1).max(50),
  deviseParDefaut: z.string().min(1).max(10),
  seuilSurcharge: nullableNumber,
  seuilSousCharge: nullableNumber,
  seuilAlerteBudget: nullableNumber,
  seuilAlerteCommande: nullableNumber,
  delaiModificationTempsJours: z.coerce.number().int().nonnegative(),
  activationValidationAbsences: z.boolean(),
  autorisationSaisieSansValorisation: z.boolean(),
})

/** Paramètres généraux (§28.2), ligne singleton : GET/PUT sans identifiant. Aucune donnée
 * financière au sens FINANCIAL_DATA_VIEW ici (seuils/paramètres, pas des montants engagés). */
export function SettingsTab() {
  const query = useQuery({ queryKey: ['settings'], queryFn: fetchSettings })
  const queryClient = useQueryClient()
  const { control, handleSubmit, reset } = useForm({ resolver: zodResolver(schema) })
  const mutation = useMutation({
    mutationFn: updateSettings,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['settings'] }),
  })

  useEffect(() => {
    if (query.data) {
      reset(query.data)
    }
  }, [query.data, reset])

  if (query.isLoading) {
    return <EmptyState title="Chargement des paramètres…" />
  }
  if (!query.data) {
    return <EmptyState title="Paramètres indisponibles" />
  }

  return (
    <Stack spacing={2}>
      <Typography variant="h6">Paramètres généraux</Typography>
      <form onSubmit={handleSubmit((values) => mutation.mutate(values))}>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <FormField control={control} name="heuresParJour" label="Heures par jour" type="number" />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <FormField control={control} name="joursOuvresParSemaine" label="Jours ouvrés par semaine" type="number" />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <FormField control={control} name="paysParDefaut" label="Pays par défaut" />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <FormField control={control} name="deviseParDefaut" label="Devise par défaut" />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <FormField control={control} name="seuilSurcharge" label="Seuil de surcharge (%)" type="number" />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <FormField control={control} name="seuilSousCharge" label="Seuil de sous-charge (%)" type="number" />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <FormField control={control} name="seuilAlerteBudget" label="Seuil d'alerte budget (%)" type="number" />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <FormField control={control} name="seuilAlerteCommande" label="Seuil d'alerte commande (%)" type="number" />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <FormField control={control} name="delaiModificationTempsJours" label="Délai de modification des temps (jours)" type="number" />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <Controller
              control={control}
              name="activationValidationAbsences"
              render={({ field }) => (
                <FormControlLabel
                  control={<Checkbox checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />}
                  label="Activer le workflow de validation des absences"
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <Controller
              control={control}
              name="autorisationSaisieSansValorisation"
              render={({ field }) => (
                <FormControlLabel
                  control={<Checkbox checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />}
                  label="Autoriser la saisie sans valorisation financière"
                />
              )}
            />
          </Grid>
        </Grid>
        <Stack direction="row" sx={{ justifyContent: 'flex-end', mt: 2 }}>
          <Button type="submit" variant="contained" loading={mutation.isPending}>
            Enregistrer
          </Button>
        </Stack>
      </form>
    </Stack>
  )
}
