import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { TestProviders } from '../../../test/testUtils'
import { ImportsPage } from './ImportsPage'
import type { ImportDiffDto } from '../../../api/types'

const {
  fetchImportTypes,
  fetchImportBatches,
  fetchImportDiffs,
  previewImport,
  simulateImport,
  executeImport,
} = vi.hoisted(() => ({
  fetchImportTypes: vi.fn(),
  fetchImportBatches: vi.fn(),
  fetchImportDiffs: vi.fn(async () => ({
    items: [] as ImportDiffDto[],
    page: 1,
    pageSize: 100,
    totalCount: 0,
  })),
  previewImport: vi.fn(),
  simulateImport: vi.fn(),
  executeImport: vi.fn(),
}))
vi.mock('../../../api/endpoints/imports', () => ({
  fetchImportTypes,
  fetchImportBatches,
  fetchImportDiffs,
  previewImport,
  simulateImport,
  executeImport,
  executeSharePointImport: vi.fn(),
}))

const importTypes = [{ type: 0, expectedHeaders: ['Nom', 'Prenom'], supportedModes: [0, 1] }]

const batch = {
  id: 'batch-1',
  type: 0,
  source: 'CSV',
  importDate: '2026-07-21T00:00:00',
  userId: 'user-1',
  mode: 0,
  fileName: 'resources.csv',
  lineCount: 1,
  addCount: 1,
  updateCount: 0,
  deleteCount: 0,
  errorCount: 0,
  status: 2,
  errors: null,
  checksum: 'abc',
  previousBatchId: null,
}

function pagedResult<T>(items: T[]) {
  return { items, page: 1, pageSize: 20, totalCount: items.length }
}

function renderPage() {
  return render(
    <TestProviders>
      <ImportsPage />
    </TestProviders>,
  )
}

describe('ImportsPage', () => {
  it('displays the import batch history', async () => {
    fetchImportTypes.mockResolvedValue(importTypes)
    fetchImportBatches.mockResolvedValue(pagedResult([batch]))

    renderPage()

    expect(await screen.findByText('resources.csv')).toBeInTheDocument()
    expect(screen.getByText('Confirmé')).toBeInTheDocument()
  })

  it('runs the full wizard flow from type selection to the execution report', async () => {
    fetchImportTypes.mockResolvedValue(importTypes)
    fetchImportBatches.mockResolvedValue(pagedResult([]))
    previewImport.mockResolvedValue({
      detectedHeaders: ['Nom', 'Prenom'],
      expectedHeaders: ['Nom', 'Prenom'],
      lineCount: 1,
      sampleRows: [{ Nom: 'BERNARD', Prenom: 'Alexandre' }],
    })
    simulateImport.mockResolvedValue({
      lineCount: 1,
      addCount: 1,
      updateCount: 0,
      unchangedCount: 0,
      deleteCount: 0,
      errorCount: 0,
      rows: [{ rowNumber: 1, entityId: null, diffType: 0, errorMessage: null, changes: [] }],
    })
    executeImport.mockResolvedValue(batch)
    const user = userEvent.setup()

    const { container } = renderPage()
    await waitFor(() => expect(fetchImportTypes).toHaveBeenCalled())

    // Étape 1 : type
    await user.click(screen.getByLabelText('Type de données'))
    await user.click(await screen.findByRole('option', { name: 'Nom, Prenom' }))
    await user.click(screen.getByRole('button', { name: 'Suivant' }))

    // Étape 2 : mode
    await user.click(screen.getByLabelText('Mode'))
    await user.click(await screen.findByRole('option', { name: 'Ajout' }))
    await user.click(screen.getByRole('button', { name: 'Suivant' }))

    // Étape 3 : fichier
    const file = new File(['Nom,Prenom\nBERNARD,Alexandre'], 'resources.csv', { type: 'text/csv' })
    const fileInput = container.querySelector('input[type="file"]')
    expect(fileInput).not.toBeNull()
    await user.upload(fileInput as HTMLInputElement, file)

    // Étape 4 : aperçu
    expect(await screen.findByText(/1 lignes/)).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Simuler' }))

    // Étape 5 : simulation
    await waitFor(() => expect(screen.getByText('Ajouts : 1')).toBeInTheDocument())
    await user.click(screen.getByRole('button', { name: 'Suivant' }))

    // Étape 6 : confirmation
    await user.click(screen.getByRole('button', { name: 'Confirmer et exécuter' }))

    // Étape 7 : compte rendu
    await waitFor(() => expect(executeImport).toHaveBeenCalledWith(0, 0, file))
    expect(await screen.findByText('Import exécuté.')).toBeInTheDocument()
  })

  it('opens the diff viewer for a past batch', async () => {
    fetchImportTypes.mockResolvedValue(importTypes)
    fetchImportBatches.mockResolvedValue(pagedResult([batch]))
    fetchImportDiffs.mockResolvedValue(
      pagedResult([
        {
          id: 'diff-1',
          importBatchId: 'batch-1',
          entityType: 'Resource',
          entityId: 'r1',
          diffType: 0,
          fieldName: null,
          oldValue: null,
          newValue: null,
        },
      ]),
    )
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('resources.csv')

    await user.click(screen.getByText('resources.csv'))

    const dialog = await screen.findByRole('dialog')
    expect(within(dialog).getByText('Resource')).toBeInTheDocument()
  })
})
