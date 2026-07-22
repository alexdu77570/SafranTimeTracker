import MenuItem from '@mui/material/MenuItem'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableRow from '@mui/material/TableRow'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { useState } from 'react'
import { fetchActivityTypes } from '../../../api/endpoints/activityTypes'
import { fetchApplications } from '../../../api/endpoints/applications'
import { fetchOrders } from '../../../api/endpoints/orders'
import { fetchDepartments, fetchServices, fetchTeams } from '../../../api/endpoints/organisation'
import { fetchProjects } from '../../../api/endpoints/projects'
import { fetchCharges, fetchDashboard } from '../../../api/endpoints/reporting'
import { fetchResources } from '../../../api/endpoints/resources'
import type { ReportingFilterQuery } from '../../../api/types'
import { ReportingPeriodType } from '../../../api/types'
import { ChartCard } from '../../../components/ui/ChartCard'
import { EmptyState } from '../../../components/ui/EmptyState'
import { FilterBar } from '../../../components/ui/FilterBar'
import { KpiBand } from '../../../components/ui/KpiBand'
import { WorkloadHeatmap } from '../../../components/ui/WorkloadHeatmap'
import { OPERATIONAL_ROLE_OPTIONS } from '../../../lib/knownReferentials'

const RUN_COLOR = '#2a78d6'
const HORS_RUN_COLOR = '#eb6834'
const MONTH_LABELS = [
  'janv.',
  'févr.',
  'mars',
  'avr.',
  'mai',
  'juin',
  'juil.',
  'août',
  'sept.',
  'oct.',
  'nov.',
  'déc.',
]

function TopEntriesTable({ rows }: { rows: { id: string; nom: string; chargeHeures: number }[] }) {
  return (
    <Table size="small">
      <TableBody>
        {rows.map((row) => (
          <TableRow key={row.id}>
            <TableCell>{row.nom}</TableCell>
            <TableCell>{row.chargeHeures} h</TableCell>
          </TableRow>
        ))}
        {!rows.length && (
          <TableRow>
            <TableCell>
              <Typography variant="body2" color="text.secondary">
                Aucune donnée.
              </Typography>
            </TableCell>
          </TableRow>
        )}
      </TableBody>
    </Table>
  )
}

/** Écran Charges (§21) : filtres complets (§21.1), indicateurs (§21.2), graphiques (§21.3) —
 * réutilise exclusivement GET /reporting/charges (Lot 5/12) et GET /reporting/dashboard (Lot 5,
 * pour "capacité vs réalisé", docs/BACKLOG_METIER.md §16) — aucune agrégation côté frontend. */
export function ChargesPage() {
  const [applicationId, setApplicationId] = useState('')
  const [projectId, setProjectId] = useState('')
  const [orderId, setOrderId] = useState('')
  const [departmentId, setDepartmentId] = useState('')
  const [serviceId, setServiceId] = useState('')
  const [teamId, setTeamId] = useState('')
  const [resourceId, setResourceId] = useState('')
  const [activityTypeId, setActivityTypeId] = useState('')
  const [operationalRoleId, setOperationalRoleId] = useState('')
  const [customFrom, setCustomFrom] = useState('2024-01-01')
  const [customTo, setCustomTo] = useState(() => new Date().toISOString().slice(0, 10))

  const applicationsQuery = useQuery({
    queryKey: ['applications', 'all'],
    queryFn: () => fetchApplications(),
  })
  const projectsQuery = useQuery({
    queryKey: ['projects', 'all'],
    queryFn: () => fetchProjects({ pageSize: 100 }),
  })
  const ordersQuery = useQuery({
    queryKey: ['orders', 'all'],
    queryFn: () => fetchOrders({ pageSize: 100 }),
  })
  const departmentsQuery = useQuery({
    queryKey: ['departments', 'all'],
    queryFn: () => fetchDepartments(),
  })
  const servicesQuery = useQuery({ queryKey: ['services', 'all'], queryFn: () => fetchServices() })
  const teamsQuery = useQuery({ queryKey: ['teams', 'all'], queryFn: () => fetchTeams() })
  const resourcesQuery = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const activityTypesQuery = useQuery({
    queryKey: ['activity-types', 'all'],
    queryFn: () => fetchActivityTypes(),
  })

  const filter: ReportingFilterQuery = {
    periodType: ReportingPeriodType.Personnalisee,
    customFrom,
    customTo,
    applicationId: applicationId || undefined,
    projectId: projectId || undefined,
    orderId: orderId || undefined,
    departmentId: departmentId || undefined,
    serviceId: serviceId || undefined,
    teamId: teamId || undefined,
    resourceId: resourceId || undefined,
    activityTypeId: activityTypeId || undefined,
    operationalRoleId: operationalRoleId || undefined,
  }
  const chargesQuery = useQuery({
    queryKey: ['reporting-charges', filter],
    queryFn: () => fetchCharges(filter),
  })
  const dashboardQuery = useQuery({
    queryKey: ['reporting-dashboard', filter],
    queryFn: () => fetchDashboard(filter),
  })

  const charges = chargesQuery.data
  const capacite = dashboardQuery.data?.operational

  const runVsHorsRunData = charges
    ? [{ label: 'Charge', RUN: charges.chargeRunHeures, 'Hors RUN': charges.chargeHorsRunHeures }]
    : []
  const evolutionData = (charges?.evolutionMensuelle ?? []).map((m) => ({
    label: `${MONTH_LABELS[m.mois - 1]} ${m.annee}`,
    RUN: m.chargeRunHeures,
    'Hors RUN': m.chargeHorsRunHeures,
  }))
  const capaciteVsRealiseData = capacite
    ? [
        {
          label: 'Période',
          'Capacité réelle': capacite.capaciteReelle,
          Réalisé: charges?.chargeTotaleHeures ?? 0,
        },
      ]
    : []
  const prevuVsRealiseData = charges?.prevuVsRealise
    ? [
        {
          label: 'Période',
          Prévu: charges.prevuVsRealise.chargePrevue ?? 0,
          Réalisé: charges.prevuVsRealise.chargeRealisee,
        },
      ]
    : []

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Charges</Typography>

      <FilterBar
        onReset={() => {
          setApplicationId('')
          setProjectId('')
          setOrderId('')
          setDepartmentId('')
          setServiceId('')
          setTeamId('')
          setResourceId('')
          setActivityTypeId('')
          setOperationalRoleId('')
        }}
      >
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
          label="Application"
          value={applicationId}
          onChange={(e) => setApplicationId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(toutes)</MenuItem>
          {(applicationsQuery.data?.items ?? []).map((a) => (
            <MenuItem key={a.id} value={a.id}>
              {a.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Projet"
          value={projectId}
          onChange={(e) => setProjectId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(projectsQuery.data?.items ?? []).map((p) => (
            <MenuItem key={p.id} value={p.id}>
              {p.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Commande"
          value={orderId}
          onChange={(e) => setOrderId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(toutes)</MenuItem>
          {(ordersQuery.data?.items ?? []).map((o) => (
            <MenuItem key={o.id} value={o.id}>
              {o.reference}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Département"
          value={departmentId}
          onChange={(e) => setDepartmentId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(departmentsQuery.data?.items ?? []).map((d) => (
            <MenuItem key={d.id} value={d.id}>
              {d.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Service"
          value={serviceId}
          onChange={(e) => setServiceId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(servicesQuery.data?.items ?? []).map((s) => (
            <MenuItem key={s.id} value={s.id}>
              {s.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Équipe"
          value={teamId}
          onChange={(e) => setTeamId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(toutes)</MenuItem>
          {(teamsQuery.data?.items ?? []).map((t) => (
            <MenuItem key={t.id} value={t.id}>
              {t.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Utilisateur"
          value={resourceId}
          onChange={(e) => setResourceId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(resourcesQuery.data?.items ?? []).map((r) => (
            <MenuItem key={r.id} value={r.id}>
              {r.prenom} {r.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Type d'activité"
          value={activityTypeId}
          onChange={(e) => setActivityTypeId(e.target.value)}
          sx={{ minWidth: 170 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(activityTypesQuery.data?.items ?? []).map((a) => (
            <MenuItem key={a.id} value={a.id}>
              {a.libelle}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Rôle opérationnel"
          value={operationalRoleId}
          onChange={(e) => setOperationalRoleId(e.target.value)}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {OPERATIONAL_ROLE_OPTIONS.map((o) => (
            <MenuItem key={o.value} value={o.value}>
              {o.label}
            </MenuItem>
          ))}
        </TextField>
      </FilterBar>

      {charges && (
        <>
          <KpiBand
            items={[
              { label: 'Charge totale', value: `${charges.chargeTotaleHeures} h` },
              { label: 'Charge RUN', value: `${charges.chargeRunHeures} h` },
              { label: 'Charge hors RUN', value: `${charges.chargeHorsRunHeures} h` },
              { label: 'Incidents', value: String(charges.nombreIncidents) },
              { label: 'Changes', value: String(charges.nombreChanges) },
              { label: 'Problems', value: String(charges.nombreProblems) },
              { label: 'RITM', value: String(charges.nombreRitm) },
              { label: 'VABE/VSR', value: `${charges.nombreVabe}/${charges.nombreVsr}` },
            ]}
          />

          <Stack
            direction="row"
            spacing={2}
            sx={{ flexWrap: 'wrap', '& > *': { flex: '1 1 320px' } }}
          >
            <ChartCard title="Top ingénieurs">
              <TopEntriesTable rows={charges.topUtilisateurs} />
            </ChartCard>
            <ChartCard title="Top applications">
              <TopEntriesTable rows={charges.topApplications} />
            </ChartCard>
            <ChartCard title="Top projets">
              <TopEntriesTable rows={charges.topProjets} />
            </ChartCard>
            <ChartCard title="Top commandes">
              <TopEntriesTable rows={charges.topCommandes} />
            </ChartCard>
          </Stack>

          <Stack
            direction="row"
            spacing={2}
            sx={{ flexWrap: 'wrap', '& > *': { flex: '1 1 320px' } }}
          >
            <ChartCard title="Répartition RUN / hors RUN">
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
                  <Bar
                    dataKey="Réalisé"
                    fill={HORS_RUN_COLOR}
                    radius={[0, 4, 4, 0]}
                    maxBarSize={24}
                  />
                </BarChart>
              </ResponsiveContainer>
            </ChartCard>
            <ChartCard title="Prévu vs réalisé">
              {charges.prevuVsRealise.chargePrevue === null ? (
                <EmptyState
                  title="Non calculable"
                  description="La planification ne porte pas de dimension commande/type d'activité."
                />
              ) : (
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
              )}
            </ChartCard>
          </Stack>

          <ChartCard title="Courbe mensuelle">
            {evolutionData.length ? (
              <ResponsiveContainer width="100%" height={260}>
                <LineChart data={evolutionData}>
                  <CartesianGrid vertical={false} stroke="#e0e0e0" />
                  <XAxis
                    dataKey="label"
                    tick={{ fill: '#52514e', fontSize: 12 }}
                    axisLine={{ stroke: '#e0e0e0' }}
                    tickLine={false}
                  />
                  <YAxis
                    tick={{ fill: '#52514e', fontSize: 12 }}
                    axisLine={false}
                    tickLine={false}
                  />
                  <Tooltip formatter={(value) => `${Number(value).toLocaleString('fr-FR')} h`} />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="RUN"
                    stroke={RUN_COLOR}
                    strokeWidth={2}
                    dot={{ r: 4 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="Hors RUN"
                    stroke={HORS_RUN_COLOR}
                    strokeWidth={2}
                    dot={{ r: 4 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            ) : (
              <EmptyState title="Aucune donnée sur la période sélectionnée." />
            )}
          </ChartCard>

          <ChartCard title="Heatmap de charge" subheader="Heures par ressource et par semaine">
            <WorkloadHeatmap entries={charges.heatmap} />
          </ChartCard>

          <Stack
            direction="row"
            spacing={2}
            sx={{ flexWrap: 'wrap', '& > *': { flex: '1 1 320px' } }}
          >
            <ChartCard title="Ressources surchargées">
              <Table size="small">
                <TableBody>
                  {charges.ressourcesSurchargees.map((r) => (
                    <TableRow key={r.resourceId}>
                      <TableCell>{r.nom}</TableCell>
                      <TableCell>{r.chargeHeures} h</TableCell>
                      <TableCell>{r.capaciteReelle} h</TableCell>
                    </TableRow>
                  ))}
                  {!charges.ressourcesSurchargees.length && (
                    <TableRow>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          Aucune ressource surchargée.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </ChartCard>
            <ChartCard title="Ressources sous-chargées">
              <Table size="small">
                <TableBody>
                  {charges.ressourcesSousChargees.map((r) => (
                    <TableRow key={r.resourceId}>
                      <TableCell>{r.nom}</TableCell>
                      <TableCell>{r.chargeHeures} h</TableCell>
                      <TableCell>{r.capaciteReelle} h</TableCell>
                    </TableRow>
                  ))}
                  {!charges.ressourcesSousChargees.length && (
                    <TableRow>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          Aucune ressource sous-chargée.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </ChartCard>
          </Stack>
        </>
      )}
    </Stack>
  )
}
