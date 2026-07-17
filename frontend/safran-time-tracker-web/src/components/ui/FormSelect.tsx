import MenuItem from '@mui/material/MenuItem'
import TextField from '@mui/material/TextField'
import { Controller, type Control, type FieldPath, type FieldValues } from 'react-hook-form'

export interface FormSelectOption {
  value: string
  label: string
}

interface FormSelectProps<TFieldValues extends FieldValues> {
  control: Control<TFieldValues>
  name: FieldPath<TFieldValues>
  label: string
  options: FormSelectOption[]
  disabled?: boolean
}

/** Liste déroulante reliée à React Hook Form, basée sur `TextField select` (cohérent avec
 * `FormField` plutôt que le `Select` MUI brut, pour partager la même gestion d'erreur). */
export function FormSelect<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  options,
  disabled = false,
}: FormSelectProps<TFieldValues>) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <TextField
          {...field}
          value={field.value ?? ''}
          select
          label={label}
          disabled={disabled}
          error={Boolean(fieldState.error)}
          helperText={fieldState.error?.message}
          fullWidth
          size="small"
        >
          {options.map((option) => (
            <MenuItem key={option.value} value={option.value}>
              {option.label}
            </MenuItem>
          ))}
        </TextField>
      )}
    />
  )
}
