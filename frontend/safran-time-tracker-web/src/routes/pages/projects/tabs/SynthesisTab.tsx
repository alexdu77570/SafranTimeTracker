import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import Grid from '@mui/material/Grid'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { fetchProjectById } from '../../../../api/endpoints/projects'
import { fetchProjectPlanningSynthesis } from '../../../../api/endpoints/projectPlanning'
import type { ProjectRiskLevel } from '../../../../api/types'
import { PermissionCodes } from '../../../../auth/permissionCodes'
import { PermissionGuard } from '../../../../auth/PermissionGuard'
import { FinancialValue } from '../../../../components/ui/FinancialValue'
import { KpiCard } from '../../../../components/ui/KpiCard'
import { StatusBadge } from '../../../../components/ui/StatusBadge'
import type { Labels } from '../useOrganisationLabels'

const riskLevelLabel: Record<ProjectRiskLevel, string> = { 0: 'Faible', 1: 'Moyen', 2: 'Élevé' }

export function SynthesisTab({
  project,
  synthesis,
  labels,
}: {
  project: NonNullable<
    ReturnType<typeof useQuery<Awaited<ReturnType<typeof fetchProjectById>>>>['data']
  >
  synthesis: Awaited<ReturnType<typeof fetchProjectPlanningSynthesis>> | undefined
  labels: Labels
}) {
  return (
    <Stack spacing={2}>
      <Card>
        <CardHeader title="Informations générales" />
        <CardContent>
          <Table size="small">
            <TableBody>
              <TableRow>
                <TableCell>Application</TableCell>
                <TableCell>{labels.applicationLabel.get(project.applicationId) ?? '—'}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Description</TableCell>
                <TableCell>{project.descriptionCourte ?? '—'}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Pilote</TableCell>
                <TableCell>{labels.resourceLabel.get(project.piloteId) ?? '—'}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Département</TableCell>
                <TableCell>{labels.departmentLabel.get(project.departmentId) ?? '—'}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Service</TableCell>
                <TableCell>{labels.serviceLabel.get(project.serviceId) ?? '—'}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Équipe</TableCell>
                <TableCell>
                  {project.teamId ? (labels.teamLabel.get(project.teamId) ?? '—') : '—'}
                </TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Client</TableCell>
                <TableCell>
                  {project.clientId ? (labels.clientLabel.get(project.clientId) ?? '—') : '—'}
                </TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Type de projet</TableCell>
                <TableCell>
                  {project.projectTypeId
                    ? (labels.projectTypeLabel.get(project.projectTypeId) ?? '—')
                    : '—'}
                </TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Statut</TableCell>
                <TableCell>
                  <StatusBadge
                    label={labels.statusLabel.get(project.statusId) ?? '—'}
                    tone="info"
                  />
                </TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Niveau de risque</TableCell>
                <TableCell>{riskLevelLabel[project.niveauRisque]}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Date de début</TableCell>
                <TableCell>{project.dateDebut}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Date de fin prévue initiale</TableCell>
                <TableCell>{project.dateFinPrevueInitiale}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Date de fin ajustée</TableCell>
                <TableCell>{project.dateFinAjustee ?? '—'}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Date de fin réelle</TableCell>
                <TableCell>{project.dateFinReelle ?? '—'}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Commentaire</TableCell>
                <TableCell>{project.commentaire ?? '—'}</TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Card>
        <CardHeader title="Écarts de charge et de planning (§29.5)" />
        <CardContent>
          {synthesis ? (
            <Grid container spacing={2}>
              <Grid size={{ xs: 6, sm: 3 }}>
                <KpiCard label="Charge initiale" value={`${synthesis.chargeInitiale} h`} />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <KpiCard
                  label="Charge ajustée"
                  value={synthesis.chargeAjustee !== null ? `${synthesis.chargeAjustee} h` : '—'}
                />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <KpiCard label="Charge restante" value={`${synthesis.chargeRestante} h`} />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <KpiCard label="Dérive planning" value={`${synthesis.derivePlanningJours} j`} />
              </Grid>
            </Grid>
          ) : (
            <Typography variant="body2" color="text.secondary">
              Synthèse de planning indisponible.
            </Typography>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader title="Synthèse financière" />
        <CardContent>
          <PermissionGuard
            code={PermissionCodes.FinancialDataView}
            fallback={
              <Typography variant="body2" color="text.disabled">
                Donnée financière non accessible.
              </Typography>
            }
          >
            {project.financialSummary ? (
              <Table size="small">
                <TableBody>
                  <TableRow>
                    <TableCell>Budget initial</TableCell>
                    <TableCell>
                      <FinancialValue value={project.financialSummary.budgetInitial} />
                    </TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>Coût réel consommé</TableCell>
                    <TableCell>
                      <FinancialValue value={project.financialSummary.coutReelConsomme} />
                    </TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>Coût contractuel consommé</TableCell>
                    <TableCell>
                      <FinancialValue value={project.financialSummary.coutContractuelConsomme} />
                    </TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>Différentiel</TableCell>
                    <TableCell>
                      <FinancialValue value={project.financialSummary.differentiel} />
                    </TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>Budget restant</TableCell>
                    <TableCell>
                      <FinancialValue value={project.financialSummary.budgetRestant} />
                    </TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>Atterrissage financier</TableCell>
                    <TableCell>
                      <FinancialValue value={synthesis?.atterrissageFinancier} />
                    </TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            ) : (
              <Typography variant="body2" color="text.secondary">
                Aucune donnée financière.
              </Typography>
            )}
          </PermissionGuard>
        </CardContent>
      </Card>
    </Stack>
  )
}
