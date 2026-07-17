import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import dayjs from 'dayjs'
import { Controller, type Control, type FieldPath, type FieldValues } from 'react-hook-form'

interface FormDatePickerProps<TFieldValues extends FieldValues> {
  control: Control<TFieldValues>
  name: FieldPath<TFieldValues>
  label: string
  disabled?: boolean
}

/** Sélecteur de date relié à React Hook Form. La valeur du formulaire reste une chaîne
 * `YYYY-MM-DD` (même format que `DateOnly` côté API, CLAUDE.md §7 : "dates ... UTC ; la
 * conversion d'affichage ... responsabilité du frontend") ; la conversion dayjs n'existe que pour
 * le rendu du composant. */
export function FormDatePicker<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  disabled = false,
}: FormDatePickerProps<TFieldValues>) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <DatePicker
          label={label}
          value={field.value ? dayjs(field.value) : null}
          onChange={(date) => field.onChange(date?.isValid() ? date.format('YYYY-MM-DD') : null)}
          disabled={disabled}
          format="DD/MM/YYYY"
          slotProps={{
            textField: {
              size: 'small',
              fullWidth: true,
              error: Boolean(fieldState.error),
              helperText: fieldState.error?.message,
              onBlur: field.onBlur,
            },
          }}
        />
      )}
    />
  )
}
