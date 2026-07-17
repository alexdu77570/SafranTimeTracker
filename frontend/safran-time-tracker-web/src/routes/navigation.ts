import {
  AppWindow,
  BarChart3,
  Building2,
  CalendarCheck,
  CalendarOff,
  CalendarRange,
  Clock,
  FileBarChart,
  Flag,
  FolderKanban,
  LayoutDashboard,
  Settings,
  ShoppingCart,
  Upload,
  Users,
  Wallet,
  type LucideIcon,
} from 'lucide-react'
import { PermissionCodes } from '../auth/permissionCodes'

export interface NavEntry {
  path: string
  label: string
  icon: LucideIcon
  /** Permission requise pour afficher l'entrée, uniquement quand elle correspond à une
   * garde réelle côté serveur (attribut de classe RequirePermission sur le contrôleur associé) —
   * ne jamais inventer une garde que l'API n'applique pas (CLAUDE.md §17). */
  requiredPermission?: string
}

/** Navigation principale (cahier des charges §8.2). Source unique partagée par la Sidebar et le
 * routeur, pour ne jamais faire diverger libellés/chemins/icônes entre les deux. */
export const navigation: NavEntry[] = [
  { path: '/', label: 'Tableau de bord', icon: LayoutDashboard },
  { path: '/temps', label: 'Temps', icon: Clock },
  { path: '/applications', label: 'Applications', icon: AppWindow },
  { path: '/projets', label: 'Projets', icon: FolderKanban },
  { path: '/planning-projet', label: 'Planning projet', icon: CalendarRange },
  { path: '/ressources', label: 'Ressources', icon: Users },
  { path: '/societes', label: 'Sociétés', icon: Building2 },
  { path: '/commandes', label: 'Commandes', icon: ShoppingCart },
  {
    path: '/budgets',
    label: 'Budgets',
    icon: Wallet,
    requiredPermission: PermissionCodes.FinancialDataView,
  },
  { path: '/charges', label: 'Charges', icon: BarChart3 },
  { path: '/disponibilites', label: 'Disponibilités', icon: CalendarCheck },
  { path: '/mes-absences', label: 'Mes absences', icon: CalendarOff },
  { path: '/jalons', label: 'Jalons', icon: Flag },
  { path: '/reporting', label: 'Reporting', icon: FileBarChart },
  {
    path: '/imports',
    label: 'Imports',
    icon: Upload,
    requiredPermission: PermissionCodes.ImportExecute,
  },
  { path: '/administration', label: 'Administration', icon: Settings },
]
