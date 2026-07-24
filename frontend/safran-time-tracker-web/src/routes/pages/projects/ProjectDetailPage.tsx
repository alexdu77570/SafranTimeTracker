import Button from '@mui/material/Button'
import Grid from '@mui/material/Grid'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { fetchMilestones } from '../../../api/endpoints/milestones'
import { fetchProjectParticipants } from '../../../api/endpoints/projectParticipants'
import { fetchProjectPlanningSynthesis } from '../../../api/endpoints/projectPlanning'
import { fetchProjectById } from '../../../api/endpoints/projects'
import { DetailPageHeader } from '../../../components/ui/DetailPageHeader'
import { DetailTabs } from '../../../components/ui/DetailTabs'
import { EmptyState } from '../../../components/ui/EmptyState'
import { KpiBand } from '../../../components/ui/KpiBand'
import { Modal } from '../../../components/ui/Modal'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { ProjectEditForm } from './ProjectForm'
import { BudgetTab } from './tabs/BudgetTab'
import { LinkedReferencesTab } from './tabs/LinkedReferencesTab'
import { MilestonesTab } from './tabs/MilestonesTab'
import { ParticipantsTab } from './tabs/ParticipantsTab'
import { PlanningTab } from './tabs/PlanningTab'
import { SynthesisTab } from './tabs/SynthesisTab'
import { TimeTab } from './tabs/TimeTab'
import { useOrganisationLabels } from './useOrganisationLabels'

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
