import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import Chip from '@mui/material/Chip'
import Grid from '@mui/material/Grid'
import IconButton from '@mui/material/IconButton'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
import { fetchApplications } from '../../../api/endpoints/applications'
import { fetchBudgets } from '../../../api/endpoints/budgets'
import { fetchMilestones } from '../../../api/endpoints/milestones'
import { fetchClients } from '../../../api/endpoints/clients'
import { fetchDepartments, fetchServices, fetchTeams } from '../../../api/endpoints/organisation'
import {
  removeProjectParticipant,
  fetchProjectParticipants,
} from '../../../api/endpoints/projectParticipants'
import {
  createAdjustedPlanVersion,
  fetchProjectPlanVersions,
  fetchProjectPlanningSynthesis,
  fetchWeeklyPlans,
  setWeeklyPlans,
} from '../../../api/endpoints/projectPlanning'
import { fetchProjectById } from '../../../api/endpoints/projects'
import { fetchProjectStatuses } from '../../../api/endpoints/projectStatuses'
import { fetchProjectTypes } from '../../../api/endpoints/projectTypes'
import { fetchProjectLinkedReferences } from '../../../api/endpoints/reporting'
import { fetchResources } from '../../../api/endpoints/resources'
import { fetchTimeEntries } from '../../../api/endpoints/timeEntries'
import {
  MilestoneStatus,
  ProjectPlanVersionStatus,
  ProjectPlanVersionType,
  ProjectRiskLevel,
} from '../../../api/types'
import { PermissionCodes } from '../../../auth/permissionCodes'
import { PermissionGuard } from '../../../auth/PermissionGuard'
import { ConfirmDialog } from '../../../components/ui/ConfirmDialog'
import { DetailPageHeader } from '../../../components/ui/DetailPageHeader'
import { DetailTabs } from '../../../components/ui/DetailTabs'
import { EmptyState } from '../../../components/ui/EmptyState'
import { FinancialValue } from '../../../components/ui/FinancialValue'
import { KpiBand } from '../../../components/ui/KpiBand'
import { KpiCard } from '../../../components/ui/KpiCard'
import { Modal } from '../../../components/ui/Modal'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { Timeline } from '../../../components/ui/Timeline'
import { WeeklyPlanningGrid } from '../../../components/ui/WeeklyPlanningGrid'
import { getOperationalRoleLabel } from '../../../lib/knownReferentials'
import { weekBounds } from '../../../lib/dateUtils'
import { MilestoneCreateForm, MilestoneEditForm } from './MilestoneForm'
import { ProjectParticipantCreateForm } from './ProjectParticipantForm'
import { ProjectEditForm } from './ProjectForm'

const riskLevelLabel: Record<ProjectRiskLevel, string> = { 0: 'Faible', 1: 'Moyen', 2: 'Élevé' }
const milestoneStatusLabel: Record<MilestoneStatus, string> = {
  0: 'À venir',
  1: 'En cours',
  2: 'Terminé',
  3: 'Annulé',
}
const milestoneStatusTone: Record<MilestoneStatus, 'neutral' | 'info' | 'success' | 'error'> = {
  0: 'neutral',
  1: 'info',
  2: 'success',
  3: 'error',
}

function useOrganisationLabels() {
  const applications = useQuery({
    queryKey: ['applications', 'all'],
    queryFn: () => fetchApplications(),
  })
  const resources = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const departments = useQuery({
    queryKey: ['departments', 'all'],
    queryFn: () => fetchDepartments(),
  })
  const services = useQuery({ queryKey: ['services', 'all'], queryFn: () => fetchServices() })
  const teams = useQuery({ queryKey: ['teams', 'all'], queryFn: () => fetchTeams() })
  const statuses = useQuery({
    queryKey: ['project-statuses', 'all'],
    queryFn: () => fetchProjectStatuses(),
  })
  const projectTypes = useQuery({
    queryKey: ['project-types', 'all'],
    queryFn: () => fetchProjectTypes(),
  })
  const clients = useQuery({ queryKey: ['clients', 'all'], queryFn: () => fetchClients() })

  return {
    applicationLabel: new Map((applications.data?.items ?? []).map((a) => [a.id, a.nom])),
    resourceLabel: new Map(
      (resources.data?.items ?? []).map((r) => [r.id, `${r.prenom} ${r.nom}`]),
    ),
    departmentLabel: new Map((departments.data?.items ?? []).map((d) => [d.id, d.nom])),
    serviceLabel: new Map((services.data?.items ?? []).map((s) => [s.id, s.nom])),
    teamLabel: new Map((teams.data?.items ?? []).map((t) => [t.id, t.nom])),
    statusLabel: new Map((statuses.data?.items ?? []).map((s) => [s.id, s.libelle])),
    projectTypeLabel: new Map((projectTypes.data?.items ?? []).map((t) => [t.id, t.libelle])),
    clientLabel: new Map((clients.data?.items ?? []).map((c) => [c.id, c.nom])),
  }
}

/** Fiche projet détaillée (§17), 7 onglets. Bandeau KPI et onglets réutilisent exclusivement les
 * endpoints déjà exposés (ProjectPlanningService pour les écarts/risques, jamais recalculés côté
 * frontend, CLAUDE.md §5). "Avancement" (%) n'est jamais calculé ici : aucune formule n'est
 * définie par le cahier des charges — affiché "—" tant qu'aucune donnée backend n'existe
 * (décision actée à l'ouverture du Lot 10, docs/BACKLOG_METIER.md). */
export function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>()
  const projectId = id ?? ''
  const queryClient = useQueryClient()
  const [tab, setTab] = useState(0)
  const [editOpen, setEditOpen] = useState(false)

  const labels = useOrganisationLabels()
  const projectQuery = useQuery({
    queryKey: ['projects', projectId],
    queryFn: () => fetchProjectById(projectId),
    enabled: Boolean(projectId),
  })
  const synthesisQuery = useQuery({
    queryKey: ['project-planning-synthesis', projectId],
    queryFn: () => fetchProjectPlanningSynthesis(projectId),
    enabled: Boolean(projectId),
  })
  const participantsQuery = useQuery({
    queryKey: ['project-participants', projectId],
    queryFn: () => fetchProjectParticipants(projectId),
    enabled: Boolean(projectId),
  })
  const milestonesQuery = useQuery({
    queryKey: ['milestones', 'byProject', projectId],
    queryFn: () => fetchMilestones({ projectId, pageSize: 100 }),
    enabled: Boolean(projectId),
  })

  if (projectQuery.isLoading) {
    return <EmptyState title="Chargement de la fiche…" />
  }
  const project = projectQuery.data
  if (!project) {
    return <EmptyState title="Projet introuvable" />
  }
  const synthesis = synthesisQuery.data

  const tabs = [
    { label: 'Synthèse' },
    { label: 'Participants' },
    { label: 'Planning' },
    { label: 'Budget' },
    { label: 'Temps' },
    { label: 'Jalons' },
    { label: 'Références liées' },
  ]

  return (
    <Stack spacing={2}>
      <DetailPageHeader
        title={project.nom}
        subtitle={project.code}
        actions={
          <Button variant="outlined" onClick={() => setEditOpen(true)}>
            Modifier
          </Button>
        }
      />

      <KpiBand
        items={[
          {
            label: 'Budget initial',
            value: project.financialSummary
              ? `${project.financialSummary.budgetInitial ?? '—'} €`
              : '—',
          },
          { label: 'Temps consommé', value: synthesis ? `${synthesis.chargeConsommee} h` : '—' },
          { label: 'Avancement', value: '—' },
          { label: 'Participants', value: String(participantsQuery.data?.totalCount ?? 0) },
          { label: 'Jalons', value: String(milestonesQuery.data?.totalCount ?? 0) },
        ]}
      >
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <Stack spacing={0.5}>
            <Typography variant="body2" color="text.secondary">
              Alertes
            </Typography>
            <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
              {synthesis?.risquePlanning && <StatusBadge label="Risque planning" tone="error" />}
              {synthesis?.risqueBudget && <StatusBadge label="Risque budget" tone="error" />}
              {!synthesis?.risquePlanning && !synthesis?.risqueBudget && (
                <StatusBadge label="Aucune" tone="success" />
              )}
            </Stack>
          </Stack>
        </Grid>
      </KpiBand>

      <DetailTabs labels={tabs.map((t) => t.label)} value={tab} onChange={setTab} />
      {tab === 0 && <SynthesisTab project={project} synthesis={synthesis} labels={labels} />}
      {tab === 1 && <ParticipantsTab projectId={projectId} />}
      {tab === 2 && <PlanningTab projectId={projectId} labels={labels} />}
      {tab === 3 && <BudgetTab projectId={projectId} />}
      {tab === 4 && <TimeTab projectId={projectId} labels={labels} />}
      {tab === 5 && <MilestonesTab projectId={projectId} labels={labels} />}
      {tab === 6 && <LinkedReferencesTab projectId={projectId} />}

      <Modal
        open={editOpen}
        title="Modifier le projet"
        onClose={() => setEditOpen(false)}
        maxWidth="md"
      >
        <ProjectEditForm
          project={project}
          onSuccess={() => {
            setEditOpen(false)
            void queryClient.invalidateQueries({ queryKey: ['projects'] })
          }}
          onCancel={() => setEditOpen(false)}
        />
      </Modal>
    </Stack>
  )
}

type Labels = ReturnType<typeof useOrganisationLabels>

function SynthesisTab({
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

function ParticipantsTab({ projectId }: { projectId: string }) {
  const queryClient = useQueryClient()
  const [createOpen, setCreateOpen] = useState(false)
  const [removeTarget, setRemoveTarget] = useState<string | null>(null)
  const resourcesQuery = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const participantsQuery = useQuery({
    queryKey: ['project-participants', projectId],
    queryFn: () => fetchProjectParticipants(projectId),
  })
  const timeEntriesQuery = useQuery({
    queryKey: ['time-entries', 'byProject', projectId],
    queryFn: () => fetchTimeEntries({ projectId, pageSize: 1000 }),
  })
  const removeMutation = useMutation({
    mutationFn: (participantId: string) => removeProjectParticipant(projectId, participantId),
    onSuccess: () => {
      setRemoveTarget(null)
      void queryClient.invalidateQueries({ queryKey: ['project-participants', projectId] })
    },
  })

  const resourceLabel = new Map(
    (resourcesQuery.data?.items ?? []).map((r) => [r.id, `${r.prenom} ${r.nom}`]),
  )

  /** Temps consommé/reste à faire par participant (§17.2) : agrégation frontend sur des saisies
   * déjà filtrées par permission (financialSnapshot absent sans FINANCIAL_DATA_VIEW, même principe
   * que la totalisation automatique du Lot 9) — aucune nouvelle règle métier serveur. */
  const consumedByResource = new Map<string, number>()
  for (const entry of timeEntriesQuery.data?.items ?? []) {
    consumedByResource.set(
      entry.resourceId,
      (consumedByResource.get(entry.resourceId) ?? 0) + entry.dureeHeures,
    )
  }

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
        <IconButton
          color="primary"
          onClick={() => setCreateOpen(true)}
          aria-label="Ajouter un participant"
        >
          <Plus size={20} />
        </IconButton>
      </Stack>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Ressource</TableCell>
            <TableCell>Rôle opérationnel</TableCell>
            <TableCell>Période</TableCell>
            <TableCell>Capacité prévue</TableCell>
            <TableCell>Temps consommé</TableCell>
            <TableCell>Reste à faire</TableCell>
            <TableCell>TJM personne</TableCell>
            <TableCell>TJM contrat</TableCell>
            <TableCell>Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(participantsQuery.data?.items ?? []).map((participant) => {
            const consomme = consumedByResource.get(participant.resourceId) ?? 0
            const resteAFaire =
              participant.capacitePrevue !== null
                ? Math.max(participant.capacitePrevue - consomme, 0)
                : null
            return (
              <TableRow key={participant.id}>
                <TableCell>
                  {resourceLabel.get(participant.resourceId) ?? participant.resourceId}
                </TableCell>
                <TableCell>{getOperationalRoleLabel(participant.operationalRoleId)}</TableCell>
                <TableCell>
                  {participant.dateDebut} — {participant.dateFin ?? 'en cours'}
                </TableCell>
                <TableCell>{participant.capacitePrevue ?? '—'}</TableCell>
                <TableCell>{consomme} h</TableCell>
                <TableCell>{resteAFaire !== null ? `${resteAFaire} h` : '—'}</TableCell>
                <TableCell>
                  <FinancialValue value={participant.financialSummary?.tjmPersonneApplicable} />
                </TableCell>
                <TableCell>
                  <FinancialValue value={participant.financialSummary?.tjmContratApplicable} />
                </TableCell>
                <TableCell>
                  <Button
                    size="small"
                    color="error"
                    onClick={() => setRemoveTarget(participant.id)}
                  >
                    Retirer
                  </Button>
                </TableCell>
              </TableRow>
            )
          })}
          {!participantsQuery.data?.items.length && (
            <TableRow>
              <TableCell colSpan={9}>
                <Typography variant="body2" color="text.secondary">
                  Aucun participant.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <Modal open={createOpen} title="Ajouter un participant" onClose={() => setCreateOpen(false)}>
        <ProjectParticipantCreateForm
          projectId={projectId}
          onSuccess={() => {
            setCreateOpen(false)
            void queryClient.invalidateQueries({ queryKey: ['project-participants', projectId] })
          }}
          onCancel={() => setCreateOpen(false)}
        />
      </Modal>

      <ConfirmDialog
        open={Boolean(removeTarget)}
        title="Retirer ce participant ?"
        description="Le participant sera désactivé (retrait logique), jamais supprimé physiquement."
        destructive
        loading={removeMutation.isPending}
        onConfirm={() => removeTarget && removeMutation.mutate(removeTarget)}
        onCancel={() => setRemoveTarget(null)}
      />
    </Stack>
  )
}

function PlanningTab({ projectId, labels }: { projectId: string; labels: Labels }) {
  const queryClient = useQueryClient()
  const [motifOpen, setMotifOpen] = useState(false)
  const [motif, setMotif] = useState('')

  const versionsQuery = useQuery({
    queryKey: ['project-plan-versions', projectId],
    queryFn: () => fetchProjectPlanVersions(projectId),
  })
  const initialVersion = versionsQuery.data?.items.find(
    (v) => v.type === ProjectPlanVersionType.Initial,
  )
  const activeAdjustedVersion = versionsQuery.data?.items.find(
    (v) => v.type === ProjectPlanVersionType.Ajuste && v.statut === ProjectPlanVersionStatus.Active,
  )

  const initialPlansQuery = useQuery({
    queryKey: ['weekly-plans', projectId, initialVersion?.id],
    queryFn: () => fetchWeeklyPlans(projectId, initialVersion?.id ?? ''),
    enabled: Boolean(initialVersion),
  })
  const adjustedPlansQuery = useQuery({
    queryKey: ['weekly-plans', projectId, activeAdjustedVersion?.id],
    queryFn: () => fetchWeeklyPlans(projectId, activeAdjustedVersion?.id ?? ''),
    enabled: Boolean(activeAdjustedVersion),
  })
  const timeEntriesQuery = useQuery({
    queryKey: ['time-entries', 'byProject', projectId],
    queryFn: () => fetchTimeEntries({ projectId, pageSize: 1000 }),
  })

  const createAdjustedMutation = useMutation({
    mutationFn: () => createAdjustedPlanVersion(projectId, { motif }),
    onSuccess: () => {
      setMotifOpen(false)
      setMotif('')
      void queryClient.invalidateQueries({ queryKey: ['project-plan-versions', projectId] })
    },
  })
  const setWeeklyPlansMutation = useMutation({
    mutationFn: ({
      versionId,
      resourceId,
      weekStartDate,
      value,
    }: {
      versionId: string
      resourceId: string
      weekStartDate: string
      value: number
    }) =>
      setWeeklyPlans(projectId, versionId, [
        { resourceId, weekStartDate, chargePlanifieeHeures: value },
      ]),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ['weekly-plans', projectId, activeAdjustedVersion?.id],
      })
    },
  })

  const { weekStartDates, rows } = useMemo(() => {
    const weeks = new Set<string>()
    const byResource = new Map<
      string,
      Record<string, { initial?: number; ajuste?: number; realise?: number; surcharge?: boolean }>
    >()

    for (const line of initialPlansQuery.data ?? []) {
      weeks.add(line.weekStartDate)
      const resourceWeeks = byResource.get(line.resourceId) ?? {}
      resourceWeeks[line.weekStartDate] = {
        ...resourceWeeks[line.weekStartDate],
        initial: line.chargePlanifieeHeures,
      }
      byResource.set(line.resourceId, resourceWeeks)
    }
    for (const line of adjustedPlansQuery.data ?? []) {
      weeks.add(line.weekStartDate)
      const resourceWeeks = byResource.get(line.resourceId) ?? {}
      resourceWeeks[line.weekStartDate] = {
        ...resourceWeeks[line.weekStartDate],
        ajuste: line.chargePlanifieeHeures,
      }
      byResource.set(line.resourceId, resourceWeeks)
    }
    for (const entry of timeEntriesQuery.data?.items ?? []) {
      const weekStart = weekBounds(entry.date).start
      weeks.add(weekStart)
      const resourceWeeks = byResource.get(entry.resourceId) ?? {}
      const cell = resourceWeeks[weekStart] ?? {}
      cell.realise = (cell.realise ?? 0) + entry.dureeHeures
      resourceWeeks[weekStart] = cell
      byResource.set(entry.resourceId, resourceWeeks)
    }

    const sortedWeeks = [...weeks].sort()
    const gridRows = [...byResource.entries()].map(([resourceId, weeksData]) => ({
      id: resourceId,
      label: labels.resourceLabel.get(resourceId) ?? resourceId,
      weeks: weeksData,
    }))
    return { weekStartDates: sortedWeeks, rows: gridRows }
  }, [initialPlansQuery.data, adjustedPlansQuery.data, timeEntriesQuery.data, labels.resourceLabel])

  return (
    <Stack spacing={2}>
      <Card>
        <CardHeader
          title="Versions de planning (§18.3)"
          action={
            <Button size="small" onClick={() => setMotifOpen(true)}>
              Nouvelle version Ajustée
            </Button>
          }
        />
        <CardContent>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Type</TableCell>
                <TableCell>Statut</TableCell>
                <TableCell>Motif</TableCell>
                <TableCell>Créée le</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {(versionsQuery.data?.items ?? []).map((version) => (
                <TableRow key={version.id}>
                  <TableCell>
                    {version.type === ProjectPlanVersionType.Initial ? 'Initiale' : 'Ajustée'}
                  </TableCell>
                  <TableCell>
                    <StatusBadge
                      label={
                        version.statut === ProjectPlanVersionStatus.Active ? 'Active' : 'Archivée'
                      }
                      tone={
                        version.statut === ProjectPlanVersionStatus.Active ? 'success' : 'neutral'
                      }
                    />
                  </TableCell>
                  <TableCell>{version.motif ?? '—'}</TableCell>
                  <TableCell>{version.createdAt}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Card>
        <CardHeader
          title="Grille hebdomadaire (§17.3, §18.2)"
          subheader="Le réalisé provient exclusivement des saisies de temps."
        />
        <CardContent>
          <WeeklyPlanningGrid
            weekStartDates={weekStartDates}
            rows={rows}
            onAjusteChange={
              activeAdjustedVersion
                ? (resourceId, weekStartDate, value) =>
                    setWeeklyPlansMutation.mutate({
                      versionId: activeAdjustedVersion.id,
                      resourceId,
                      weekStartDate,
                      value,
                    })
                : undefined
            }
          />
        </CardContent>
      </Card>

      <Modal open={motifOpen} title="Nouvelle version Ajustée" onClose={() => setMotifOpen(false)}>
        <Stack spacing={2}>
          <Chip label="Motif obligatoire (§18.3)" size="small" sx={{ alignSelf: 'flex-start' }} />
          <TextField
            label="Motif"
            value={motif}
            onChange={(e) => setMotif(e.target.value)}
            multiline
            rows={2}
            fullWidth
            size="small"
          />
          <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
            <Button onClick={() => setMotifOpen(false)}>Annuler</Button>
            <Button
              variant="contained"
              disabled={!motif.trim()}
              loading={createAdjustedMutation.isPending}
              onClick={() => createAdjustedMutation.mutate()}
            >
              Créer
            </Button>
          </Stack>
        </Stack>
      </Modal>
    </Stack>
  )
}

function BudgetTab({ projectId }: { projectId: string }) {
  const budgetsQuery = useQuery({
    queryKey: ['budgets', 'byProject', projectId],
    queryFn: () => fetchBudgets({ projectId }),
  })

  return (
    <PermissionGuard
      code={PermissionCodes.FinancialDataView}
      fallback={
        <Typography variant="body2" color="text.disabled">
          Donnée financière non accessible.
        </Typography>
      }
    >
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Nom</TableCell>
            <TableCell>Initial</TableCell>
            <TableCell>Ajusté</TableCell>
            <TableCell>Coût réel</TableCell>
            <TableCell>Différentiel</TableCell>
            <TableCell>Reste à consommer</TableCell>
            <TableCell>Atterrissage estimé</TableCell>
            <TableCell>Statut</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(budgetsQuery.data?.items ?? []).map((budget) => (
            <TableRow key={budget.id}>
              <TableCell>{budget.name}</TableCell>
              <TableCell>
                <FinancialValue value={budget.initialAmount} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.adjustedAmount} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.coutReelConsomme} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.differentiel} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.montantRestant} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.atterrissageEstime} />
              </TableCell>
              <TableCell>
                <StatusBadge
                  label={budget.status === 0 ? 'Actif' : 'Clôturé'}
                  tone={budget.risqueDepassement ? 'error' : 'success'}
                />
              </TableCell>
            </TableRow>
          ))}
          {!budgetsQuery.data?.items.length && (
            <TableRow>
              <TableCell colSpan={8}>
                <Typography variant="body2" color="text.secondary">
                  Aucun budget lié à ce projet.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </PermissionGuard>
  )
}

function TimeTab({ projectId, labels }: { projectId: string; labels: Labels }) {
  const timeEntriesQuery = useQuery({
    queryKey: ['time-entries', 'byProject', 'detail', projectId],
    queryFn: () => fetchTimeEntries({ projectId, pageSize: 200 }),
  })
  const entries = timeEntriesQuery.data?.items ?? []

  const byPersonne = new Map<string, number>()
  const bySemaine = new Map<string, number>()
  for (const entry of entries) {
    byPersonne.set(entry.resourceId, (byPersonne.get(entry.resourceId) ?? 0) + entry.dureeHeures)
    const weekStart = weekBounds(entry.date).start
    bySemaine.set(weekStart, (bySemaine.get(weekStart) ?? 0) + entry.dureeHeures)
  }

  return (
    <Stack spacing={2}>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardHeader title="Temps par personne" />
            <CardContent>
              <Table size="small">
                <TableBody>
                  {[...byPersonne.entries()].map(([resourceId, heures]) => (
                    <TableRow key={resourceId}>
                      <TableCell>{labels.resourceLabel.get(resourceId) ?? resourceId}</TableCell>
                      <TableCell>{heures} h</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardHeader title="Temps par semaine" />
            <CardContent>
              <Table size="small">
                <TableBody>
                  {[...bySemaine.entries()].sort().map(([week, heures]) => (
                    <TableRow key={week}>
                      <TableCell>{week}</TableCell>
                      <TableCell>{heures} h</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Card>
        <CardHeader title="Temps détaillés" />
        <CardContent>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>Ressource</TableCell>
                <TableCell>Durée</TableCell>
                <TableCell>Référence</TableCell>
                <TableCell>Modifié le</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {entries.map((entry) => (
                <TableRow key={entry.id}>
                  <TableCell>{entry.date}</TableCell>
                  <TableCell>
                    {labels.resourceLabel.get(entry.resourceId) ?? entry.resourceId}
                  </TableCell>
                  <TableCell>{entry.dureeHeures} h</TableCell>
                  <TableCell>{entry.reference ?? '—'}</TableCell>
                  <TableCell>{entry.updatedAt ?? entry.createdAt}</TableCell>
                </TableRow>
              ))}
              {!entries.length && (
                <TableRow>
                  <TableCell colSpan={5}>
                    <Typography variant="body2" color="text.secondary">
                      Aucune saisie de temps sur ce projet.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </Stack>
  )
}

function MilestonesTab({ projectId, labels }: { projectId: string; labels: Labels }) {
  const queryClient = useQueryClient()
  const [createOpen, setCreateOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<string | null>(null)
  const milestonesQuery = useQuery({
    queryKey: ['milestones', 'byProject', projectId],
    queryFn: () => fetchMilestones({ projectId, pageSize: 100 }),
  })
  const items = milestonesQuery.data?.items ?? []
  const editing = items.find((m) => m.id === editTarget)

  const enRetard = items.filter((m) => m.estEnRetard).length
  const aVenir = items.filter((m) => m.statut === MilestoneStatus.AVenir).length
  const termines = items.filter((m) => m.statut === MilestoneStatus.Termine).length

  return (
    <Stack spacing={2}>
      <Grid container spacing={2}>
        <Grid size={{ xs: 4 }}>
          <KpiCard label="En retard" value={String(enRetard)} />
        </Grid>
        <Grid size={{ xs: 4 }}>
          <KpiCard label="À venir" value={String(aVenir)} />
        </Grid>
        <Grid size={{ xs: 4 }}>
          <KpiCard label="Terminés" value={String(termines)} />
        </Grid>
      </Grid>

      <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
        <IconButton color="primary" onClick={() => setCreateOpen(true)} aria-label="Créer un jalon">
          <Plus size={20} />
        </IconButton>
      </Stack>

      <Timeline
        items={items.map((m) => ({
          id: m.id,
          date: m.datePrevue,
          label: m.nom,
          sublabel: labels.resourceLabel.get(m.responsableId),
          statusLabel: milestoneStatusLabel[m.statut],
          statusTone: m.estEnRetard ? 'error' : milestoneStatusTone[m.statut],
          highlighted: m.estEnRetard,
        }))}
        emptyLabel="Aucun jalon pour ce projet."
      />

      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Nom</TableCell>
            <TableCell>Date prévue</TableCell>
            <TableCell>Statut</TableCell>
            <TableCell>Dépend de</TableCell>
            <TableCell>Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {items.map((m) => (
            <TableRow
              key={m.id}
              hover
              onClick={() => setEditTarget(m.id)}
              sx={{ cursor: 'pointer' }}
            >
              <TableCell>{m.nom}</TableCell>
              <TableCell>{m.datePrevue}</TableCell>
              <TableCell>
                <StatusBadge
                  label={milestoneStatusLabel[m.statut]}
                  tone={m.estEnRetard ? 'error' : milestoneStatusTone[m.statut]}
                />
              </TableCell>
              <TableCell>
                {m.dependsOnMilestoneId
                  ? (items.find((o) => o.id === m.dependsOnMilestoneId)?.nom ?? '—')
                  : '—'}
              </TableCell>
              <TableCell>
                <Button
                  size="small"
                  onClick={(e) => {
                    e.stopPropagation()
                    setEditTarget(m.id)
                  }}
                >
                  Modifier
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Modal open={createOpen} title="Créer un jalon" onClose={() => setCreateOpen(false)}>
        <MilestoneCreateForm
          projectId={projectId}
          onSuccess={() => {
            setCreateOpen(false)
            void queryClient.invalidateQueries({ queryKey: ['milestones', 'byProject', projectId] })
          }}
          onCancel={() => setCreateOpen(false)}
        />
      </Modal>

      {editing && (
        <Modal
          open={Boolean(editing)}
          title="Modifier le jalon"
          onClose={() => setEditTarget(null)}
        >
          <MilestoneEditForm
            milestone={editing}
            onSuccess={() => {
              setEditTarget(null)
              void queryClient.invalidateQueries({
                queryKey: ['milestones', 'byProject', projectId],
              })
            }}
            onCancel={() => setEditTarget(null)}
          />
        </Modal>
      )}
    </Stack>
  )
}

function LinkedReferencesTab({ projectId }: { projectId: string }) {
  const referencesQuery = useQuery({
    queryKey: ['project-linked-references', projectId],
    queryFn: () => fetchProjectLinkedReferences(projectId),
  })

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Référence</TableCell>
          <TableCell>Type</TableCell>
          <TableCell>Nombre de saisies</TableCell>
          <TableCell>Charge</TableCell>
          <TableCell>Première date</TableCell>
          <TableCell>Dernière date</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {(referencesQuery.data ?? []).map((ref) => (
          <TableRow key={ref.reference}>
            <TableCell>{ref.reference}</TableCell>
            <TableCell>{ref.activityTypeLibelle}</TableCell>
            <TableCell>{ref.nombreSaisies}</TableCell>
            <TableCell>{ref.chargeHeures} h</TableCell>
            <TableCell>{ref.premiereDate}</TableCell>
            <TableCell>{ref.derniereDate}</TableCell>
          </TableRow>
        ))}
        {!referencesQuery.data?.length && (
          <TableRow>
            <TableCell colSpan={6}>
              <Typography variant="body2" color="text.secondary">
                Aucune référence RUN rattachée à ce projet.
              </Typography>
            </TableCell>
          </TableRow>
        )}
      </TableBody>
    </Table>
  )
}
