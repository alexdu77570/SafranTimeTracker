import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import Checkbox from '@mui/material/Checkbox'
import FormControlLabel from '@mui/material/FormControlLabel'
import MenuItem from '@mui/material/MenuItem'
import Step from '@mui/material/Step'
import StepLabel from '@mui/material/StepLabel'
import Stepper from '@mui/material/Stepper'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import {
  executeImport,
  executeSharePointImport,
  fetchImportTypes,
  previewImport,
  simulateImport,
} from '../../api/endpoints/imports'
import type { ImportBatchDto, ImportPreviewDto, ImportSimulationDto } from '../../api/types'
import { ImportMode } from '../../api/types'
import { getErrorMessage } from '../../api/errors'
import { StatusBadge } from './StatusBadge'

const modeLabels: Record<ImportMode, string> = {
  [ImportMode.Ajout]: 'Ajout',
  [ImportMode.MiseAJour]: 'Mise à jour',
  [ImportMode.Complet]: 'Complet',
}

type WizardStep = 'type' | 'mode' | 'file' | 'preview' | 'simulate' | 'confirm' | 'result'
const stepOrder: WizardStep[] = ['type', 'mode', 'file', 'preview', 'simulate', 'confirm', 'result']
const stepLabels: Record<WizardStep, string> = {
  type: 'Type',
  mode: 'Mode',
  file: 'Fichier',
  preview: 'Aperçu',
  simulate: 'Simulation',
  confirm: 'Confirmation',
  result: 'Compte rendu',
}

interface ImportWizardProps {
  onCompleted: () => void
}

/** Assistant d'import (§27.3), `ImportWizard` anticipé par docs/ARCHITECTURE.md §3, construit au
 * Lot 12. Le mapping des colonnes (étape 5) reste volontairement en lecture seule (en-têtes
 * détectés vs attendus), décision reconfirmée à l'ouverture du lot (docs/BACKLOG_METIER.md §16,
 * décision 4) — aucune réassociation interactive. */
export function ImportWizard({ onCompleted }: ImportWizardProps) {
  const [step, setStep] = useState<WizardStep>('type')
  const [typeId, setTypeId] = useState('')
  const [mode, setMode] = useState('')
  const [file, setFile] = useState<File | null>(null)
  const [sharePoint, setSharePoint] = useState(false)
  const [preview, setPreview] = useState<ImportPreviewDto | null>(null)
  const [simulation, setSimulation] = useState<ImportSimulationDto | null>(null)
  const [result, setResult] = useState<ImportBatchDto | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const typesQuery = useQuery({ queryKey: ['import-types'], queryFn: () => fetchImportTypes() })
  const selectedType = typesQuery.data?.find((t) => String(t.type) === typeId)

  function reset() {
    setStep('type')
    setTypeId('')
    setMode('')
    setFile(null)
    setSharePoint(false)
    setPreview(null)
    setSimulation(null)
    setResult(null)
    setError(null)
  }

  async function handleFileChosen(chosenFile: File) {
    if (!selectedType) return
    setFile(chosenFile)
    setError(null)
    setLoading(true)
    try {
      const dto = await previewImport(selectedType.type, chosenFile)
      setPreview(dto)
      setStep('preview')
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  async function handleSimulate() {
    if (!selectedType || !file || mode === '') return
    setError(null)
    setLoading(true)
    try {
      const dto = await simulateImport(selectedType.type, Number(mode) as ImportMode, file)
      setSimulation(dto)
      setStep('simulate')
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  async function handleExecute() {
    if (!selectedType || !file || mode === '') return
    setError(null)
    setLoading(true)
    try {
      const dto = sharePoint
        ? await executeSharePointImport(selectedType.type, Number(mode) as ImportMode, file)
        : await executeImport(selectedType.type, Number(mode) as ImportMode, file)
      setResult(dto)
      setStep('result')
      onCompleted()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  return (
    <Stack spacing={2}>
      <Stepper activeStep={stepOrder.indexOf(step)}>
        {stepOrder.map((s) => (
          <Step key={s}>
            <StepLabel>{stepLabels[s]}</StepLabel>
          </Step>
        ))}
      </Stepper>

      {error && <Alert severity="error">{error}</Alert>}

      {step === 'type' && (
        <Stack spacing={2}>
          <TextField
            select
            label="Type de données"
            value={typeId}
            onChange={(e) => setTypeId(e.target.value)}
            fullWidth
          >
            {(typesQuery.data ?? []).map((t) => (
              <MenuItem key={t.type} value={String(t.type)}>
                {t.expectedHeaders.join(', ')}
              </MenuItem>
            ))}
          </TextField>
          <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
            <Button variant="contained" disabled={!selectedType} onClick={() => setStep('mode')}>
              Suivant
            </Button>
          </Stack>
        </Stack>
      )}

      {step === 'mode' && selectedType && (
        <Stack spacing={2}>
          <TextField
            select
            label="Mode"
            value={mode}
            onChange={(e) => setMode(e.target.value)}
            fullWidth
          >
            {selectedType.supportedModes.map((m) => (
              <MenuItem key={m} value={String(m)}>
                {modeLabels[m]}
              </MenuItem>
            ))}
          </TextField>
          <FormControlLabel
            control={
              <Checkbox checked={sharePoint} onChange={(e) => setSharePoint(e.target.checked)} />
            }
            label="Import SharePoint simulé (§27.4)"
          />
          <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
            <Button onClick={() => setStep('type')}>Précédent</Button>
            <Button variant="contained" disabled={mode === ''} onClick={() => setStep('file')}>
              Suivant
            </Button>
          </Stack>
        </Stack>
      )}

      {step === 'file' && (
        <Stack spacing={2}>
          <Typography variant="body2" color="text.secondary">
            En-têtes attendues : {selectedType?.expectedHeaders.join(', ')}
          </Typography>
          <Button variant="outlined" component="label" loading={loading}>
            Choisir un fichier CSV
            <input
              type="file"
              accept=".csv"
              hidden
              onChange={(e) => {
                const chosen = e.target.files?.[0]
                if (chosen) void handleFileChosen(chosen)
              }}
            />
          </Button>
          <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
            <Button onClick={() => setStep('mode')}>Précédent</Button>
          </Stack>
        </Stack>
      )}

      {step === 'preview' && preview && (
        <Stack spacing={2}>
          <Typography variant="subtitle2">
            En-têtes détectées vs attendues (lecture seule, docs/BACKLOG_METIER.md §16)
          </Typography>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Détectées</TableCell>
                <TableCell>Attendues</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {preview.expectedHeaders.map((header, index) => (
                <TableRow key={header}>
                  <TableCell>{preview.detectedHeaders[index] ?? '—'}</TableCell>
                  <TableCell>{header}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <Typography variant="subtitle2">Aperçu ({preview.lineCount} lignes)</Typography>
          <Table size="small">
            <TableBody>
              {preview.sampleRows.slice(0, 5).map((row, index) => (
                <TableRow key={index}>
                  <TableCell>{Object.values(row).join(' | ')}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
            <Button onClick={() => setStep('file')}>Précédent</Button>
            <Button variant="contained" loading={loading} onClick={handleSimulate}>
              Simuler
            </Button>
          </Stack>
        </Stack>
      )}

      {step === 'simulate' && simulation && (
        <Stack spacing={2}>
          <Stack direction="row" spacing={2}>
            <Typography variant="body2">Ajouts : {simulation.addCount}</Typography>
            <Typography variant="body2">Mises à jour : {simulation.updateCount}</Typography>
            <Typography variant="body2">Inchangés : {simulation.unchangedCount}</Typography>
            <Typography variant="body2">Suppressions : {simulation.deleteCount}</Typography>
            <Typography
              variant="body2"
              color={simulation.errorCount > 0 ? 'error' : 'text.secondary'}
            >
              Erreurs : {simulation.errorCount}
            </Typography>
          </Stack>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Ligne</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>Erreur</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {simulation.rows.slice(0, 20).map((row) => (
                <TableRow key={row.rowNumber}>
                  <TableCell>{row.rowNumber}</TableCell>
                  <TableCell>
                    <StatusBadge
                      label={String(row.diffType)}
                      tone={row.errorMessage ? 'error' : 'info'}
                    />
                  </TableCell>
                  <TableCell>{row.errorMessage ?? '—'}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
            <Button onClick={() => setStep('preview')}>Précédent</Button>
            <Button
              variant="contained"
              disabled={simulation.errorCount > 0}
              onClick={() => setStep('confirm')}
            >
              Suivant
            </Button>
          </Stack>
        </Stack>
      )}

      {step === 'confirm' && (
        <Stack spacing={2}>
          <Alert severity="warning">
            Confirmer l'exécution de cet import {sharePoint ? '(source SharePoint simulée) ' : ''}
            écrira réellement les données.
          </Alert>
          <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
            <Button onClick={() => setStep('simulate')}>Précédent</Button>
            <Button variant="contained" color="error" loading={loading} onClick={handleExecute}>
              Confirmer et exécuter
            </Button>
          </Stack>
        </Stack>
      )}

      {step === 'result' && result && (
        <Stack spacing={2}>
          <Alert severity="success">Import exécuté.</Alert>
          <Table size="small">
            <TableBody>
              <TableRow>
                <TableCell>Fichier</TableCell>
                <TableCell>{result.fileName}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Ajouts</TableCell>
                <TableCell>{result.addCount}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Mises à jour</TableCell>
                <TableCell>{result.updateCount}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Suppressions</TableCell>
                <TableCell>{result.deleteCount}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Erreurs</TableCell>
                <TableCell>{result.errorCount}</TableCell>
              </TableRow>
            </TableBody>
          </Table>
          <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
            <Button variant="contained" onClick={reset}>
              Nouvel import
            </Button>
          </Stack>
        </Stack>
      )}
    </Stack>
  )
}
