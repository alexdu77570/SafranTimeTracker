import Button from '@mui/material/Button'
import MenuItem from '@mui/material/MenuItem'
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
  exportFinancial,
  exportOperational,
  fetchFinancialReport,
  fetchOperationalReport,
} from '../../../api/endpoints/reporting'
import type { ReportingFilterQuery } from '../../../api/types'
import { ExportFormat, ReportingPeriodType } from '../../../api/types'
import { PermissionCodes } from '../../../auth/permissionCodes'
import { PermissionGuard } from '../../../auth/PermissionGuard'
import { ChartCard } from '../../../components/ui/ChartCard'
import { FilterBar } from '../../../components/ui/FilterBar'
import { FinancialValue } from '../../../components/ui/FinancialValue'

const exportFormatOptions = [
  { value: String(ExportFormat.Csv), label: 'CSV' },
  { value: String(ExportFormat.Excel), label: 'Excel' },
  { value: String(ExportFormat.Pdf), label: 'PDF' },
]

/** Écran Reporting (§26) : rapport opérationnel (§26.1, ouvert à tous) + rapport financier (§26.2,
 * PermissionGuard) avec exports réels (§26.3) — réutilise GET /reporting/operational et
 * GET /reporting/financial (Lot 5/12) sans aucune agrégation côté frontend. */
export function ReportingPage() {
  const [customFrom, setCustomFrom] = useState('2024-01-01')
  const [customTo, setCustomTo] = useState(() => new Date().toISOString().slice(0, 10))
  const [exportFormat, setExportFormat] = useState(String(ExportFormat.Csv))
  const [exporting, setExporting] = useState<string | null>(null)

  const filter: ReportingFilterQuery = {
    periodType: ReportingPeriodType.Personnalisee,
    customFrom,
    customTo,
  }
  const operationalQuery = useQuery({
    queryKey: ['reporting-operational', filter],
    queryFn: () => fetchOperationalReport(filter),
  })
  const financialQuery = useQuery({
    queryKey: ['reporting-financial', filter],
    queryFn: () => fetchFinancialReport(filter),
  })

  const format = Number(exportFormat) as ExportFormat
  const report = operationalQuery.data
  const financial = financialQuery.data

  async function handleExportOperational() {
    setExporting('operational')
    try {
      await exportOperational(filter, format)
    } finally {
      setExporting(null)
    }
  }

  async function handleExportFinancial() {
    setExporting('financial')
    try {
      await exportFinancial(filter, format)
    } finally {
      setExporting(null)
    }
  }

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Reporting</Typography>

      <FilterBar onReset={() => {}}>
        <TextField
          size="small"
          type="date"
          label="Du"
          value={customFrom}
          onChange={(e) => setCustomFrom(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          size="small"
          type="date"
          label="Au"
          value={customTo}
          onChange={(e) => setCustomTo(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          select
          size="small"
          label="Format d'export"
          value={exportFormat}
          onChange={(e) => setExportFormat(e.target.value)}
          sx={{ minWidth: 150 }}
        >
          {exportFormatOptions.map((o) => (
            <MenuItem key={o.value} value={o.value}>
              {o.label}
            </MenuItem>
          ))}
        </TextField>
      </FilterBar>

      <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="h6">Rapport opérationnel (§26.1)</Typography>
        <Button
          variant="outlined"
          loading={exporting === 'operational'}
          onClick={handleExportOperational}
        >
          Exporter
        </Button>
      </Stack>

      {report && (
        <Stack
          direction="row"
          spacing={2}
          sx={{ flexWrap: 'wrap', '& > *': { flex: '1 1 320px' } }}
        >
          <ChartCard title="Charge par équipe/service/département">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Nom</TableCell>
                  <TableCell>Charge</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {[
                  ...report.chargeParEquipe,
                  ...report.chargeParService,
                  ...report.chargeParDepartement,
                ].map((g) => (
                  <TableRow key={g.id}>
                    <TableCell>{g.nom}</TableCell>
                    <TableCell>{g.chargeHeures} h</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </ChartCard>
          <ChartCard title="Jalons en retard">
            <Table size="small">
              <TableBody>
                {report.jalonsEnRetard.map((m) => (
                  <TableRow key={m.id}>
                    <TableCell>{m.nom}</TableCell>
                    <TableCell>{m.datePrevue}</TableCell>
                  </TableRow>
                ))}
                {!report.jalonsEnRetard.length && (
                  <TableRow>
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">
                        Aucun jalon en retard.
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </ChartCard>
          <ChartCard title="Capacité et disponibilité">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Ressource</TableCell>
                  <TableCell>Capacité réelle</TableCell>
                  <TableCell>Taux</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {report.capaciteEtDisponibilite.map((c) => (
                  <TableRow key={c.resourceId}>
                    <TableCell>{c.nom}</TableCell>
                    <TableCell>{c.capaciteReelle} h</TableCell>
                    <TableCell>{c.tauxDisponibilite}%</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </ChartCard>
        </Stack>
      )}

      <PermissionGuard
        code={PermissionCodes.FinancialDataView}
        fallback={
          <Typography variant="body2" color="text.disabled">
            Donnée financière non accessible.
          </Typography>
        }
      >
        <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
          <Typography variant="h6">Rapport financier (§26.2)</Typography>
          <Button
            variant="outlined"
            loading={exporting === 'financial'}
            onClick={handleExportFinancial}
          >
            Exporter
          </Button>
        </Stack>

        {financial && (
          <Stack
            direction="row"
            spacing={2}
            sx={{ flexWrap: 'wrap', '& > *': { flex: '1 1 320px' } }}
          >
            <ChartCard title="Différentiel par projet/commande/société/ressource">
              <Table size="small">
                <TableBody>
                  {[
                    ...financial.differentielParProjet,
                    ...financial.differentielParCommande,
                    ...financial.differentielParSociete,
                    ...financial.differentielParRessource,
                  ].map((e) => (
                    <TableRow key={e.id}>
                      <TableCell>{e.nom}</TableCell>
                      <TableCell>
                        <FinancialValue value={e.differentiel} />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </ChartCard>
            <ChartCard title="Besoins de rallonge">
              <Table size="small">
                <TableBody>
                  {financial.besoinsRallonge.map((o) => (
                    <TableRow key={o.orderId}>
                      <TableCell>{o.reference}</TableCell>
                      <TableCell>
                        <FinancialValue value={o.coutReelConsomme} />
                      </TableCell>
                    </TableRow>
                  ))}
                  {!financial.besoinsRallonge.length && (
                    <TableRow>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          Aucun besoin de rallonge.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </ChartCard>
            <ChartCard title="Commandes à renouveler">
              <Table size="small">
                <TableBody>
                  {financial.commandesARenouveler.map((o) => (
                    <TableRow key={o.orderId}>
                      <TableCell>{o.reference}</TableCell>
                      <TableCell>{o.dateFinAjustee ?? '—'}</TableCell>
                    </TableRow>
                  ))}
                  {!financial.commandesARenouveler.length && (
                    <TableRow>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          Aucune commande à renouveler.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </ChartCard>
          </Stack>
        )}
      </PermissionGuard>
    </Stack>
  )
}
