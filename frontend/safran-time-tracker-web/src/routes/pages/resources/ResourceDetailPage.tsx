import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import Chip from '@mui/material/Chip'
import Grid from '@mui/material/Grid'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'
import { fetchDepartments, fetchServices, fetchTeams } from '../../../api/endpoints/organisation'
import { fetchPermissions } from '../../../api/endpoints/permissions'
import { fetchResourceTjmHistory } from '../../../api/endpoints/resourceTjmHistory'
import { fetchResourceById } from '../../../api/endpoints/resources'
import { fetchUsers } from '../../../api/endpoints/users'
import { ReferentialStatus } from '../../../api/types'
import { PermissionGuard } from '../../../auth/PermissionGuard'
import { PermissionCodes } from '../../../auth/permissionCodes'
import { EmptyState } from '../../../components/ui/EmptyState'
import { FinancialValue } from '../../../components/ui/FinancialValue'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { getRoleLabel } from '../../../lib/knownReferentials'

/**
 * Fiche Ressource/Utilisateur à 4 sections (docs/ROADMAP.md, Lot 8) : Général, Organisation,
 * Sécurité (identité/rôle/permissions du compte utilisateur lié, le cas échéant), Historique des
 * TJM (sous PermissionGuard FINANCIAL_DATA_VIEW — la garde réelle reste côté serveur, ce composant
 * n'affiche que ce que l'API renvoie, CLAUDE.md §17).
 */
export function ResourceDetailPage() {
  const { id } = useParams<{ id: string }>()
  const resourceId = id ?? ''

  const resourceQuery = useQuery({ queryKey: ['resources', resourceId], queryFn: () => fetchResourceById(resourceId), enabled: Boolean(resourceId) })
  const departmentsQuery = useQuery({ queryKey: ['departments', 'all'], queryFn: () => fetchDepartments() })
  const servicesQuery = useQuery({ queryKey: ['services', 'all'], queryFn: () => fetchServices() })
  const teamsQuery = useQuery({ queryKey: ['teams', 'all'], queryFn: () => fetchTeams() })
  const usersQuery = useQuery({ queryKey: ['users', 'all'], queryFn: () => fetchUsers() })
  const permissionsQuery = useQuery({ queryKey: ['permissions', 'all'], queryFn: () => fetchPermissions() })
  const tjmQuery = useQuery({
    queryKey: ['resource-tjm-history', resourceId],
    queryFn: () => fetchResourceTjmHistory(resourceId),
    enabled: Boolean(resourceId),
  })

  if (resourceQuery.isLoading) {
    return <EmptyState title="Chargement de la fiche…" />
  }
  const resource = resourceQuery.data
  if (!resource) {
    return <EmptyState title="Ressource introuvable" />
  }

  const department = departmentsQuery.data?.items.find((d) => d.id === resource.departmentId)
  const service = servicesQuery.data?.items.find((s) => s.id === resource.serviceId)
  const team = teamsQuery.data?.items.find((t) => t.id === resource.teamId)
  const linkedUser = usersQuery.data?.items.find((u) => u.resourceId === resource.id)
  const permissionById = new Map(permissionsQuery.data?.items.map((p) => [p.id, p]) ?? [])

  return (
    <Stack spacing={3}>
      <Typography variant="h5">
        {resource.prenom} {resource.nom}
      </Typography>

      <Card>
        <CardHeader title="Général" />
        <CardContent>
          <Table size="small">
            <TableBody>
              <TableRow>
                <TableCell>Nom</TableCell>
                <TableCell>{resource.nom}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Prénom</TableCell>
                <TableCell>{resource.prenom}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Statut</TableCell>
                <TableCell>
                  <StatusBadge
                    label={resource.statut === ReferentialStatus.Actif ? 'Actif' : 'Inactif'}
                    tone={resource.statut === ReferentialStatus.Actif ? 'success' : 'neutral'}
                  />
                </TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Commentaire</TableCell>
                <TableCell>{resource.commentaire ?? '—'}</TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Card>
        <CardHeader title="Organisation" />
        <CardContent>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Table size="small">
                <TableBody>
                  <TableRow>
                    <TableCell>Département</TableCell>
                    <TableCell>{department?.nom ?? '—'}</TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>Service</TableCell>
                    <TableCell>{service?.nom ?? '—'}</TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>Équipe</TableCell>
                    <TableCell>{team?.nom ?? '—'}</TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Table size="small">
                <TableBody>
                  <TableRow>
                    <TableCell>Capacité journalière</TableCell>
                    <TableCell>{resource.dailyCapacity} h</TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>Capacité hebdomadaire</TableCell>
                    <TableCell>{resource.weeklyCapacity} h</TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Card>
        <CardHeader title="Sécurité" />
        <CardContent>
          {linkedUser ? (
            <Stack spacing={1.5}>
              <Table size="small">
                <TableBody>
                  <TableRow>
                    <TableCell>Identifiant</TableCell>
                    <TableCell>{linkedUser.identifiant}</TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>Email</TableCell>
                    <TableCell>{linkedUser.email}</TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>Rôle</TableCell>
                    <TableCell>{getRoleLabel(linkedUser.roleId)}</TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>Statut du compte</TableCell>
                    <TableCell>
                      <StatusBadge
                        label={linkedUser.statut === ReferentialStatus.Actif ? 'Actif' : 'Inactif'}
                        tone={linkedUser.statut === ReferentialStatus.Actif ? 'success' : 'neutral'}
                      />
                    </TableCell>
                  </TableRow>
                </TableBody>
              </Table>
              <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
                {linkedUser.permissionIds.map((permissionId) => {
                  const permission = permissionById.get(permissionId)
                  return permission ? <Chip key={permissionId} label={permission.code} size="small" /> : null
                })}
              </Stack>
            </Stack>
          ) : (
            <Typography variant="body2" color="text.secondary">
              Aucun compte utilisateur rattaché à cette ressource.
            </Typography>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader title="Historique des TJM" />
        <CardContent>
          <PermissionGuard
            code={PermissionCodes.FinancialDataView}
            fallback={
              <Typography variant="body2" color="text.disabled">
                Donnée financière non accessible.
              </Typography>
            }
          >
            {tjmQuery.data?.items.length ? (
              <Table size="small">
                <TableBody>
                  {tjmQuery.data.items.map((entry) => (
                    <TableRow key={entry.id}>
                      <TableCell>
                        {entry.startDate} → {entry.endDate ?? 'en cours'}
                      </TableCell>
                      <TableCell>
                        <FinancialValue value={entry.dailyRate} />
                      </TableCell>
                      <TableCell>
                        <StatusBadge
                          label={entry.status === ReferentialStatus.Actif ? 'Actif' : 'Clôturé'}
                          tone={entry.status === ReferentialStatus.Actif ? 'success' : 'neutral'}
                        />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            ) : (
              <Typography variant="body2" color="text.secondary">
                Aucun historique de TJM.
              </Typography>
            )}
          </PermissionGuard>
        </CardContent>
      </Card>
    </Stack>
  )
}
