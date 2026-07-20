import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'
import { fetchCompanyById, fetchCompanyContracts } from '../../../api/endpoints/companies'
import { ReferentialStatus } from '../../../api/types'
import { PermissionCodes } from '../../../auth/permissionCodes'
import { PermissionGuard } from '../../../auth/PermissionGuard'
import { EmptyState } from '../../../components/ui/EmptyState'
import { FinancialValue } from '../../../components/ui/FinancialValue'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { COMPANY_TYPE_OPTIONS } from '../../../lib/knownReferentials'

function CompanyContractsTable({ companyId }: { companyId: string }) {
  const query = useQuery({ queryKey: ['company-contracts', companyId], queryFn: () => fetchCompanyContracts(companyId) })

  if (query.isLoading) {
    return <Typography variant="body2">Chargement…</Typography>
  }
  if (!query.data?.items.length) {
    return (
      <Typography variant="body2" color="text.secondary">
        Aucun contrat.
      </Typography>
    )
  }

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Numéro</TableCell>
          <TableCell>Début</TableCell>
          <TableCell>Fin</TableCell>
          <TableCell>TJM contractuel</TableCell>
          <TableCell>Statut</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {query.data.items.map((contract) => (
          <TableRow key={contract.id}>
            <TableCell>{contract.contractNumber ?? '—'}</TableCell>
            <TableCell>{contract.startDate}</TableCell>
            <TableCell>{contract.endDate ?? 'en cours'}</TableCell>
            <TableCell>
              <FinancialValue value={contract.contractDailyRate} />
            </TableCell>
            <TableCell>
              <StatusBadge
                label={contract.status === ReferentialStatus.Actif ? 'Actif' : 'Clôturé'}
                tone={contract.status === ReferentialStatus.Actif ? 'success' : 'neutral'}
              />
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}

/** Fiche Société (§12.1) + historique des contrats, confidentiel (docs/ROADMAP.md, Lot 8) — gardé
 * par FINANCIAL_DATA_VIEW côté serveur (§12.4), 403 explicite sans la permission, jamais un
 * masquage visuel côté client (CLAUDE.md §17). */
export function CompanyDetailPage() {
  const { id } = useParams<{ id: string }>()
  const companyId = id ?? ''
  const query = useQuery({ queryKey: ['companies', companyId], queryFn: () => fetchCompanyById(companyId), enabled: Boolean(companyId) })

  if (query.isLoading) {
    return <EmptyState title="Chargement de la fiche…" />
  }
  const company = query.data
  if (!company) {
    return <EmptyState title="Société introuvable" />
  }

  return (
    <Stack spacing={3}>
      <Typography variant="h5">{company.nom}</Typography>

      <Card>
        <CardHeader title="Informations générales" />
        <CardContent>
          <Table size="small">
            <TableBody>
              <TableRow>
                <TableCell>Code</TableCell>
                <TableCell>{company.code}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Type</TableCell>
                <TableCell>{COMPANY_TYPE_OPTIONS.find((o) => o.value === company.companyTypeId)?.label ?? company.companyTypeId}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Contact principal</TableCell>
                <TableCell>{company.contactPrincipal}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Email</TableCell>
                <TableCell>{company.emailContact}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Téléphone</TableCell>
                <TableCell>{company.telephone ?? '—'}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Adresse</TableCell>
                <TableCell>{company.adresse ?? '—'}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Statut</TableCell>
                <TableCell>
                  <StatusBadge
                    label={company.statut === ReferentialStatus.Actif ? 'Actif' : 'Inactif'}
                    tone={company.statut === ReferentialStatus.Actif ? 'success' : 'neutral'}
                  />
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Card>
        <CardHeader title="Historique des contrats (confidentiel)" />
        <CardContent>
          <PermissionGuard
            code={PermissionCodes.FinancialDataView}
            fallback={
              <Typography variant="body2" color="text.disabled">
                Donnée financière non accessible.
              </Typography>
            }
          >
            <CompanyContractsTable companyId={company.id} />
          </PermissionGuard>
        </CardContent>
      </Card>
    </Stack>
  )
}
