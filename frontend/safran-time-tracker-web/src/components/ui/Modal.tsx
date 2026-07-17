import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import IconButton from '@mui/material/IconButton'
import Stack from '@mui/material/Stack'
import { X } from 'lucide-react'
import type { ReactNode } from 'react'

interface ModalProps {
  open: boolean
  title: string
  onClose: () => void
  children: ReactNode
  actions?: ReactNode
  maxWidth?: 'xs' | 'sm' | 'md' | 'lg'
}

export function Modal({ open, title, onClose, children, actions, maxWidth = 'sm' }: ModalProps) {
  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth={maxWidth}>
      <DialogTitle>
        <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
          {title}
          <IconButton size="small" onClick={onClose} aria-label="Fermer">
            <X size={18} />
          </IconButton>
        </Stack>
      </DialogTitle>
      <DialogContent dividers>{children}</DialogContent>
      {actions && <DialogActions>{actions}</DialogActions>}
    </Dialog>
  )
}
