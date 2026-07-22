import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { Bar, BarChart, Legend, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { fetchMilestones } from '../../../api/endpoints/milestones'
import {
  fetchCharges,
  fetchDashboard,
  fetchFinancialReport,
} from '../../../api/endpoints/reporting'
import { MilestoneStatus } from '../../../api/types'
import { PermissionCodes } from '../../../auth/permissionCodes'
import { PermissionGuard } from '../../../auth/PermissionGuard'
import { ChartCard } from '../../../components/ui/ChartCard'
import { FinancialValue } from '../../../components/ui/FinancialValue'
import { KpiBand } from '../../../components/ui/KpiBand'

const RUN_COLOR = '#2a78d6'
const HORS_RUN_COLOR = '#eb6834'

/** Tableau de bord (§25) : KPI opérationnels (toujours) + financiers (PermissionGuard), graphiques
 * §25.3 entièrement recomposés à partir des endpoints déjà exposés (Charges, Dashboard, Financial,
 * Milestones) — aucune agrégation dupliquée côté frontend, jamais un widget configurable ce lot. */
export function DashboardPage() {
  const today = dayjs().format('YYYY-MM-DD')
  const dashboardQuery = useQuery({
    queryKey: ['reporting-dashboard', 'default'],
    queryFn: () => fetchDashboard(),
  })
  const chargesQuery = useQuery({
    queryKey: ['reporting-charges', 'default'],
    queryFn: () => fetchCharges({}),
  })
  const financialQuery = useQuery({
    queryKey: ['reporting-financial', 'default'],
    queryFn: () => fetchFinancialReport(),
  })
  const milestonesQuery = useQuery({
    queryKey: ['milestones', 'upcoming'],
    queryFn: () => fetchMilestones({ pageSize: 200 }),
  })

  const operational = dashboardQuery.data?.operational
  const financial = dashboardQuery.data?.financial
  const charges = chargesQuery.data
  const report = financialQuery.data

  const in30Days = dayjs().add(30, 'day').format('YYYY-MM-DD')
  const jalonsAVenir = (milestonesQuery.data?.items ?? [])
    .filter(
      (m) =>
        m.datePrevue >= today &&
        m.datePrevue <= in30Days &&
        m.statut !== MilestoneStatus.Termine &&
        m.statut !== MilestoneStatus.Annule,
    )
    .sort((a, b) => a.datePrevue.localeCompare(b.datePrevue))
    .slice(0, 8)

  const runVsHorsRunData = operational
    ? [
        {
          label: 'Charge',
          RUN: operational.chargeRunHeures,
          'Hors RUN': operational.chargeHorsRunHeures,
        },
      ]
    : []
  const evolutionData = (charges?.evolutionMensuelle ?? []).slice(-6)
  const capaciteVsRealiseData = operational
    ? [
        {
          label: 'Période',
          'Capacité réelle': operational.capaciteReelle,
          Réalisé: operational.tempsSaisisHeures,
        },
      ]
    : []
  const prevuVsRealiseData =
    charges?.prevuVsRealise.chargePrevue !== null && charges?.prevuVsRealise
      ? [
          {
            label: 'Période',
            Prévu: charges.prevuVsRealise.chargePrevue,
            Réalisé: charges.prevuVsRealise.chargeRealisee,
          },
        ]
      : []

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Tableau de bord</Typography>

      {operational && (
        <KpiBand
          items={[
            { label: 'Temps saisis', value: `${operational.tempsSaisisHeures} h` },
            { label: 'Capacité théorique', value: `${operational.capaciteTheorique} h` },
            { label: 'Capacité réelle', value: `${operational.capaciteReelle} h` },
            { label: 'Taux de disponibilité', value: `${operational.tauxDisponibilite}%` },
            { label: 'Incidents ouverts', value: String(operational.incidentsOuverts) },
            { label: 'Changes en cours', value: String(operational.changesEnCours) },
            { label: 'Problems ouverts', value: String(operational.problemsOuverts) },
            { label: 'RITM en cours', value: String(operational.ritmEnCours) },
            { label: 'Projets actifs', value: String(operational.projetsActifs) },
            { label: 'Jalons en retard', value: String(operational.jalonsEnRetard) },
            { label: 'Ressources surchargées', value: String(operational.ressourcesSurchargees) },
            {
              label: 'Ressources sous-chargées',
              value: String(operational.ressourcesSousChargees),
            },
          ]}
        />
      )}

      <PermissionGuard code={PermissionCodes.FinancialDataView} fallback={null}>
        {financial && (
          <KpiBand
            items={[
              { label: 'Budget initial total', value: `${financial.budgetInitialTotal} €` },
              { label: 'Budget ajusté total', value: `${financial.budgetAjusteTotal} €` },
              { label: 'Coût réel total', value: `${financial.coutReelTotal} €` },
              { label: 'Coût contractuel total', value: `${financial.coutContractuelTotal} €` },
              { label: 'Différentiel global', value: `${financial.differentielGlobal} €` },
              { label: 'Budget restant', value: `${financial.budgetRestant} €` },
              { label: 'Commandes à risque', value: String(financial.commandesARisque) },
              { label: 'Projets sous-financés', value: String(financial.projetsSousFinances) },
              { label: 'Atterrissage estimé', value: `${financial.atterrissageEstime} €` },
            ]}
          />
        )}
      </PermissionGuard>

      <Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap', '& > *': { flex: '1 1 320px' } }}>
        <ChartCard title="RUN vs hors RUN">
          <ResponsiveContainer width="100%" height={120}>
            <BarChart data={runVsHorsRunData} layout="vertical" stackOffset="expand">
              <XAxis type="number" hide />
              <YAxis type="category" dataKey="label" hide />
              <Tooltip formatter={(value) => `${Number(value).toLocaleString('fr-FR')} h`} />
              <Legend />
              <Bar dataKey="RUN" stackId="a" fill={RUN_COLOR} radius={[4, 0, 0, 4]} />
              <Bar dataKey="Hors RUN" stackId="a" fill={HORS_RUN_COLOR} radius={[0, 4, 4, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Capacité vs réalisé">
          <ResponsiveContainer width="100%" height={120}>
            <BarChart data={capaciteVsRealiseData} layout="vertical">
              <XAxis type="number" />
              <YAxis type="category" dataKey="label" hide />
              <Tooltip formatter={(value) => `${Number(value).toLocaleString('fr-FR')} h`} />
              <Legend />
              <Bar
                dataKey="Capacité réelle"
                fill={RUN_COLOR}
                radius={[0, 4, 4, 0]}
                maxBarSize={24}
              />
              <Bar dataKey="Réalisé" fill={HORS_RUN_COLOR} radius={[0, 4, 4, 0]} maxBarSize={24} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Prévu vs réalisé">
          {prevuVsRealiseData.length ? (
            <ResponsiveContainer width="100%" height={120}>
              <BarChart data={prevuVsRealiseData} layout="vertical">
                <XAxis type="number" />
                <YAxis type="category" dataKey="label" hide />
                <Tooltip formatter={(value) => `${Number(value).toLocaleString('fr-FR')} h`} />
                <Legend />
                <Bar dataKey="Prévu" fill={RUN_COLOR} radius={[0, 4, 4, 0]} maxBarSize={24} />
                <Bar
                  dataKey="Réalisé"
                  fill={HORS_RUN_COLOR}
                  radius={[0, 4, 4, 0]}
                  maxBarSize={24}
                />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <Typography variant="body2" color="text.secondary">
              Non calculable sur ce périmètre.
            </Typography>
          )}
        </ChartCard>
      </Stack>

      <Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap', '& > *': { flex: '1 1 320px' } }}>
        <ChartCard title="Charge par équipe">
          <Table size="small">
            <TableBody>
              {(charges?.topUtilisateurs ?? []).map((e) => (
                <TableRow key={e.id}>
                  <TableCell>{e.nom}</TableCell>
                  <TableCell>{e.chargeHeures} h</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </ChartCard>
        <ChartCard title="Charge par application">
          <Table size="small">
            <TableBody>
              {(charges?.topApplications ?? []).map((e) => (
                <TableRow key={e.id}>
                  <TableCell>{e.nom}</TableCell>
                  <TableCell>{e.chargeHeures} h</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </ChartCard>
        <ChartCard title="Jalons à venir (30 jours)">
          <Table size="small">
            <TableBody>
              {jalonsAVenir.map((m) => (
                <TableRow key={m.id}>
                  <TableCell>{m.nom}</TableCell>
                  <TableCell>{m.datePrevue}</TableCell>
                </TableRow>
              ))}
              {!jalonsAVenir.length && (
                <TableRow>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">
                      Aucun jalon à venir.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </ChartCard>
      </Stack>

      <ChartCard title="Évolution mensuelle (6 derniers mois avec saisie)">
        <Table size="small">
          <TableBody>
            {evolutionData.map((m) => (
              <TableRow key={`${m.annee}-${m.mois}`}>
                <TableCell>
                  {m.mois}/{m.annee}
                </TableCell>
                <TableCell>{m.chargeTotaleHeures} h</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </ChartCard>

      <PermissionGuard code={PermissionCodes.FinancialDataView} fallback={null}>
        {report && (
          <Stack
            direction="row"
            spacing={2}
            sx={{ flexWrap: 'wrap', '& > *': { flex: '1 1 320px' } }}
          >
            <ChartCard title="Budget par projet">
              <Table size="small">
                <TableBody>
                  {report.differentielParProjet.map((e) => (
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
            <ChartCard title="Budget par commande">
              <Table size="small">
                <TableBody>
                  {report.differentielParCommande.map((e) => (
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
            <ChartCard title="Différentiel par société">
              <Table size="small">
                <TableBody>
                  {report.differentielParSociete.map((e) => (
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
          </Stack>
        )}
      </PermissionGuard>
    </Stack>
  )
}
