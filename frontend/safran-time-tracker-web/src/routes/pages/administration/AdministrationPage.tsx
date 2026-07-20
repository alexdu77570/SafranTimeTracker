import Box from '@mui/material/Box'
import Tab from '@mui/material/Tab'
import Tabs from '@mui/material/Tabs'
import { useState, type SyntheticEvent } from 'react'
import type { GridColDef } from '@mui/x-data-grid'
import { fetchApplications } from '../../../api/endpoints/applications'
import { fetchClients } from '../../../api/endpoints/clients'
import { fetchCompanies } from '../../../api/endpoints/companies'
import { fetchCostCenters } from '../../../api/endpoints/costCenters'
import { fetchCurrencies } from '../../../api/endpoints/currencies'
import { fetchMilestoneTypes } from '../../../api/endpoints/milestoneTypes'
import { fetchActivityTypes } from '../../../api/endpoints/activityTypes'
import { fetchDepartments, fetchServices, fetchTeams } from '../../../api/endpoints/organisation'
import { fetchProjectTypes } from '../../../api/endpoints/projectTypes'
import { fetchTechnologies } from '../../../api/endpoints/technologies'
import type {
  ActivityTypeDto,
  ApplicationReferenceDto,
  ClientDto,
  CompanyDto,
  CostCenterDto,
  CurrencyDto,
  DepartmentDto,
  MilestoneTypeDto,
  ProjectTypeDto,
  ServiceDto,
  TeamDto,
  TechnologyDto,
} from '../../../api/types'
import { ReferentialStatus } from '../../../api/types'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { ReferentialAdminTab } from './ReferentialAdminTab'
import { ActivityTypeCreateForm } from './forms/ActivityTypeForm'
import { ClientCreateForm, ClientEditForm } from './forms/ClientForm'
import { CompanyCreateForm, CompanyEditForm } from './forms/CompanyForm'
import { CostCenterCreateForm, CostCenterEditForm } from './forms/CostCenterForm'
import { CurrencyCreateForm, CurrencyEditForm } from './forms/CurrencyForm'
import { MilestoneTypeCreateForm } from './forms/MilestoneTypeForm'
import { DepartmentCreateForm, ServiceCreateForm, TeamCreateForm } from './forms/OrganisationForms'
import { ProjectTypeCreateForm, ProjectTypeEditForm } from './forms/ProjectTypeForm'
import { TechnologyCreateForm, TechnologyEditForm } from './forms/TechnologyForm'
import { AbsenceTypesTab } from './tabs/AbsenceTypesTab'
import { AuditTab } from './tabs/AuditTab'
import { OrdersTab } from './tabs/OrdersTab'
import { PermissionsTab } from './tabs/PermissionsTab'
import { SettingsTab } from './tabs/SettingsTab'
import { UsersTab } from './tabs/UsersTab'

function statutColumn<T extends { statut: ReferentialStatus }>(): GridColDef<T> {
  return {
    field: 'statut',
    headerName: 'Statut',
    width: 110,
    renderCell: (params) => (
      <StatusBadge
        label={params.value === ReferentialStatus.Actif ? 'Actif' : 'Inactif'}
        tone={params.value === ReferentialStatus.Actif ? 'success' : 'neutral'}
      />
    ),
  }
}

const departmentColumns: GridColDef<DepartmentDto>[] = [
  { field: 'code', headerName: 'Code', width: 120 },
  { field: 'nom', headerName: 'Nom', flex: 1 },
  statutColumn(),
]

const serviceColumns: GridColDef<ServiceDto>[] = [
  { field: 'code', headerName: 'Code', width: 120 },
  { field: 'nom', headerName: 'Nom', flex: 1 },
  statutColumn(),
]

const teamColumns: GridColDef<TeamDto>[] = [
  { field: 'code', headerName: 'Code', width: 120 },
  { field: 'nom', headerName: 'Nom', flex: 1 },
  statutColumn(),
]

const applicationColumns: GridColDef<ApplicationReferenceDto>[] = [
  { field: 'code', headerName: 'Code', width: 120 },
  { field: 'nom', headerName: 'Nom', flex: 1 },
  statutColumn(),
]

const activityTypeColumns: GridColDef<ActivityTypeDto>[] = [
  { field: 'code', headerName: 'Code', width: 140 },
  { field: 'libelle', headerName: 'Libellé', flex: 1 },
  { field: 'isRun', headerName: 'RUN', width: 90, valueFormatter: (v: boolean) => (v ? 'Oui' : 'Non') },
  statutColumn(),
]

const milestoneTypeColumns: GridColDef<MilestoneTypeDto>[] = [
  { field: 'code', headerName: 'Code', width: 140 },
  { field: 'libelle', headerName: 'Libellé', flex: 1 },
  statutColumn(),
]

const companyColumns: GridColDef<CompanyDto>[] = [
  { field: 'code', headerName: 'Code', width: 100 },
  { field: 'nom', headerName: 'Nom', flex: 1 },
  { field: 'contactPrincipal', headerName: 'Contact', width: 160 },
  statutColumn(),
]

const technologyColumns: GridColDef<TechnologyDto>[] = [
  { field: 'code', headerName: 'Code', width: 140 },
  { field: 'libelle', headerName: 'Libellé', flex: 1 },
  statutColumn(),
]

const clientColumns: GridColDef<ClientDto>[] = [
  { field: 'code', headerName: 'Code', width: 120 },
  { field: 'nom', headerName: 'Nom', flex: 1 },
  statutColumn(),
]

const projectTypeColumns: GridColDef<ProjectTypeDto>[] = [
  { field: 'code', headerName: 'Code', width: 140 },
  { field: 'libelle', headerName: 'Libellé', flex: 1 },
  statutColumn(),
]

const costCenterColumns: GridColDef<CostCenterDto>[] = [
  { field: 'code', headerName: 'Code', width: 140 },
  { field: 'libelle', headerName: 'Libellé', flex: 1 },
  statutColumn(),
]

const currencyColumns: GridColDef<CurrencyDto>[] = [
  { field: 'codeIso', headerName: 'Code ISO', width: 100 },
  { field: 'libelle', headerName: 'Libellé', flex: 1 },
  { field: 'symbole', headerName: 'Symbole', width: 100 },
  statutColumn(),
]

interface TabDef {
  label: string
  content: React.ReactNode
}

/**
 * Panneau Administration (cahier des charges, Lot 8 — docs/ROADMAP.md) : 13 onglets d'origine
 * (Utilisateurs, Département, Services, Équipes, Applications, Types d'activités, Types
 * d'absences, Types de jalons, Sociétés, Commandes, Paramètres, Permissions, Audit) + 5 nouveaux
 * référentiels validés (docs/BACKLOG_METIER.md §5-9) : Technologies, Clients, Types de projet,
 * Centres de coûts, Devises.
 */
export function AdministrationPage() {
  const [tab, setTab] = useState(0)
  const handleChange = (_: SyntheticEvent, value: number) => setTab(value)

  const tabs: TabDef[] = [
    { label: 'Utilisateurs', content: <UsersTab /> },
    {
      label: 'Département',
      content: (
        <ReferentialAdminTab
          title="Départements"
          queryKey="departments-admin"
          fetchList={fetchDepartments}
          columns={departmentColumns}
          renderCreateForm={(props) => <DepartmentCreateForm {...props} />}
        />
      ),
    },
    {
      label: 'Services',
      content: (
        <ReferentialAdminTab
          title="Services"
          queryKey="services-admin"
          fetchList={fetchServices}
          columns={serviceColumns}
          renderCreateForm={(props) => <ServiceCreateForm {...props} />}
        />
      ),
    },
    {
      label: 'Équipes',
      content: (
        <ReferentialAdminTab
          title="Équipes"
          queryKey="teams-admin"
          fetchList={fetchTeams}
          columns={teamColumns}
          renderCreateForm={(props) => <TeamCreateForm {...props} />}
        />
      ),
    },
    {
      label: 'Applications',
      content: (
        <ReferentialAdminTab
          title="Applications"
          queryKey="applications-admin"
          fetchList={fetchApplications}
          columns={applicationColumns}
        />
      ),
    },
    {
      label: "Types d'activités",
      content: (
        <ReferentialAdminTab
          title="Types d'activités"
          queryKey="activity-types-admin"
          fetchList={fetchActivityTypes}
          columns={activityTypeColumns}
          renderCreateForm={(props) => <ActivityTypeCreateForm {...props} />}
        />
      ),
    },
    { label: "Types d'absences", content: <AbsenceTypesTab /> },
    {
      label: 'Types de jalons',
      content: (
        <ReferentialAdminTab
          title="Types de jalons"
          queryKey="milestone-types-admin"
          fetchList={fetchMilestoneTypes}
          columns={milestoneTypeColumns}
          renderCreateForm={(props) => <MilestoneTypeCreateForm {...props} />}
        />
      ),
    },
    {
      label: 'Sociétés',
      content: (
        <ReferentialAdminTab
          title="Sociétés"
          queryKey="companies-admin"
          fetchList={fetchCompanies}
          columns={companyColumns}
          renderCreateForm={(props) => <CompanyCreateForm {...props} />}
          renderEditForm={(props) => <CompanyEditForm {...props} />}
        />
      ),
    },
    { label: 'Commandes', content: <OrdersTab /> },
    { label: 'Paramètres', content: <SettingsTab /> },
    { label: 'Permissions', content: <PermissionsTab /> },
    { label: 'Audit', content: <AuditTab /> },
    {
      label: 'Technologies',
      content: (
        <ReferentialAdminTab
          title="Technologies"
          description="Rattachées aux applications et aux ressources (docs/BACKLOG_METIER.md §5)."
          queryKey="technologies-admin"
          fetchList={fetchTechnologies}
          columns={technologyColumns}
          renderCreateForm={(props) => <TechnologyCreateForm {...props} />}
          renderEditForm={(props) => <TechnologyEditForm {...props} />}
        />
      ),
    },
    {
      label: 'Clients',
      content: (
        <ReferentialAdminTab
          title="Clients"
          description="Donneur d'ordre d'un projet, distinct de la société prestataire (docs/BACKLOG_METIER.md §6)."
          queryKey="clients-admin"
          fetchList={fetchClients}
          columns={clientColumns}
          renderCreateForm={(props) => <ClientCreateForm {...props} />}
          renderEditForm={(props) => <ClientEditForm {...props} />}
        />
      ),
    },
    {
      label: 'Types de projet',
      content: (
        <ReferentialAdminTab
          title="Types de projet"
          description="Axe de classification indépendant du statut de cycle de vie (docs/BACKLOG_METIER.md §7)."
          queryKey="project-types-admin"
          fetchList={fetchProjectTypes}
          columns={projectTypeColumns}
          renderCreateForm={(props) => <ProjectTypeCreateForm {...props} />}
          renderEditForm={(props) => <ProjectTypeEditForm {...props} />}
        />
      ),
    },
    {
      label: 'Centres de coûts',
      content: (
        <ReferentialAdminTab
          title="Centres de coûts"
          description="Axe organisationnel analytique, rattaché à un département et/ou un service (docs/BACKLOG_METIER.md §8)."
          queryKey="cost-centers-admin"
          fetchList={fetchCostCenters}
          columns={costCenterColumns}
          renderCreateForm={(props) => <CostCenterCreateForm {...props} />}
          renderEditForm={(props) => <CostCenterEditForm {...props} />}
        />
      ),
    },
    {
      label: 'Devises',
      content: (
        <ReferentialAdminTab
          title="Devises"
          description="Référentiel de consultation, sans impact sur les calculs financiers (docs/BACKLOG_METIER.md §9)."
          queryKey="currencies-admin"
          fetchList={fetchCurrencies}
          columns={currencyColumns}
          renderCreateForm={(props) => <CurrencyCreateForm {...props} />}
          renderEditForm={(props) => <CurrencyEditForm {...props} />}
        />
      ),
    },
  ]

  return (
    <Box>
      <Tabs value={tab} onChange={handleChange} variant="scrollable" scrollButtons="auto" sx={{ mb: 2 }}>
        {tabs.map((t) => (
          <Tab key={t.label} label={t.label} />
        ))}
      </Tabs>
      {tabs[tab]?.content}
    </Box>
  )
}
