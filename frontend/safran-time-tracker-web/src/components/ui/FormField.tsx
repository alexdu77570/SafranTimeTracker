import TextField, { type TextFieldProps } from '@mui/material/TextField'
import { Controller, type Control, type FieldPath, type FieldValues } from 'react-hook-form'

interface FormFieldProps<TFieldValues extends FieldValues> {
  control: Control<TFieldValues>
  name: FieldPath<TFieldValues>
  label: string
  type?: TextFieldProps['type']
  multiline?: boolean
  rows?: number
  disabled?: boolean
}

/**
 * Champ de saisie texte relié à React Hook Form (CLAUDE.md §9). Les schémas de validation Zod
 * restent définis par chaque formulaire ; ce composant se contente d'afficher l'erreur déjà
 * résolue par RHF (`fieldState.error`), jamais sa propre logique de validation.
 */
export function FormField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  type = 'text',
  multiline = false,
  rows,
  disabled = false,
}: FormFieldProps<TFieldValues>) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <TextField
          {...field}
          value={field.value ?? ''}
          label={label}
          type={type}
          multiline={multiline}
          rows={rows}
          disabled={disabled}
          error={Boolean(fieldState.error)}
          helperText={fieldState.error?.message}
          fullWidth
          size="small"
        />
      )}
    />
  )
}
