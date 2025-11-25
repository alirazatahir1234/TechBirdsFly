# Frontend Integration - Export Service

Guide to integrating the Export Service download functionality in your Next.js frontend.

## 1. Create Export Store

Create `lib/store/exportStore.ts`:

```typescript
import { create } from 'zustand';

interface ExportState {
  isExporting: boolean;
  error: string | null;
  downloadCode: (projectId: string, framework: 'html' | 'react' | 'nextjs') => Promise<void>;
  clearError: () => void;
}

export const useExportStore = create<ExportState>((set) => ({
  isExporting: false,
  error: null,

  downloadCode: async (projectId: string, framework: 'html' | 'react' | 'nextjs') => {
    set({ isExporting: true, error: null });

    try {
      const response = await fetch(
        `${process.env.NEXT_PUBLIC_API_BASE}/export/${projectId}/${framework}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
          }
        }
      );

      if (!response.ok) {
        throw new Error(`Export failed: ${response.statusText}`);
      }

      const result = await response.json();

      // Trigger download
      const downloadUrl = `${process.env.NEXT_PUBLIC_API_BASE}${result.downloadUrl}`;
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.download = `${projectId}-${framework}.zip`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);

      set({ isExporting: false });
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unknown error occurred';
      set({ 
        isExporting: false,
        error: message
      });
      console.error('Export error:', error);
    }
  },

  clearError: () => set({ error: null })
}));
```

## 2. Create Download Buttons Component

Create `components/export/ExportButtons.tsx`:

```typescript
'use client';

import { useExportStore } from '@/lib/store/exportStore';
import { Button } from '@/components/ui/button';
import { Loader2, Download, AlertCircle } from 'lucide-react';

interface ExportButtonsProps {
  projectId: string;
  projectName: string;
}

export function ExportButtons({ projectId, projectName }: ExportButtonsProps) {
  const { isExporting, error, downloadCode, clearError } = useExportStore();

  const frameworks = [
    { value: 'html', label: '📄 HTML', description: 'Plain HTML/CSS' },
    { value: 'react', label: '⚛️ React', description: 'React JSX' },
    { value: 'nextjs', label: '▲ Next.js', description: 'Next.js App Router' }
  ] as const;

  const handleDownload = async (framework: 'html' | 'react' | 'nextjs') => {
    clearError();
    await downloadCode(projectId, framework);
  };

  return (
    <div className="space-y-4">
      {error && (
        <div className="flex items-center gap-2 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700">
          <AlertCircle className="w-5 h-5" />
          <span>{error}</span>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
        {frameworks.map((fw) => (
          <Button
            key={fw.value}
            onClick={() => handleDownload(fw.value)}
            disabled={isExporting}
            className="flex items-center gap-2"
            variant="outline"
          >
            {isExporting ? (
              <Loader2 className="w-4 h-4 animate-spin" />
            ) : (
              <Download className="w-4 h-4" />
            )}
            {fw.label}
          </Button>
        ))}
      </div>

      <p className="text-sm text-gray-500">
        Download {projectName} code in your preferred framework
      </p>
    </div>
  );
}
```

## 3. Add to Project Dashboard

Update your project page to include export buttons:

```typescript
// app/dashboard/projects/[id]/page.tsx
'use client';

import { ExportButtons } from '@/components/export/ExportButtons';

export default function ProjectPage({ params }: { params: { id: string } }) {
  const projectId = params.id;
  const projectName = 'My Awesome Website'; // Fetch from API

  return (
    <div className="space-y-6">
      <div>
        <h1>{projectName}</h1>
        <p>Project ID: {projectId}</p>
      </div>

      <section className="bg-white p-6 rounded-lg border">
        <h2 className="text-xl font-bold mb-4">Export Code</h2>
        <ExportButtons projectId={projectId} projectName={projectName} />
      </section>

      {/* Other project content */}
    </div>
  );
}
```

## 4. Add to Context Menu

Create a quick-access export menu:

```typescript
// components/export/ExportMenu.tsx
'use client';

import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu';
import { Download } from 'lucide-react';
import { useExportStore } from '@/lib/store/exportStore';

interface ExportMenuProps {
  projectId: string;
}

export function ExportMenu({ projectId }: ExportMenuProps) {
  const { downloadCode, isExporting } = useExportStore();

  return (
    <DropdownMenu>
      <Button variant="ghost" size="sm">
        <Download className="w-4 h-4" />
      </Button>

      <DropdownMenuContent align="end">
        <DropdownMenuLabel>Export as</DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem
          onClick={() => downloadCode(projectId, 'html')}
          disabled={isExporting}
        >
          📄 HTML
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => downloadCode(projectId, 'react')}
          disabled={isExporting}
        >
          ⚛️ React
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => downloadCode(projectId, 'nextjs')}
          disabled={isExporting}
        >
          ▲ Next.js
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
```

## 5. Table Row Actions

Add export to project list:

```typescript
// components/projects/ProjectsTable.tsx
import { ExportMenu } from '@/components/export/ExportMenu';

export function ProjectsTable({ projects }: { projects: Project[] }) {
  return (
    <table>
      <tbody>
        {projects.map((project) => (
          <tr key={project.id}>
            <td>{project.name}</td>
            <td>{project.framework}</td>
            <td>
              <div className="flex gap-2">
                <Button variant="ghost" size="sm">Edit</Button>
                <ExportMenu projectId={project.id} />
                <Button variant="ghost" size="sm">Delete</Button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
```

## 6. Real-time Export Progress

For large projects, show progress:

```typescript
// components/export/ExportProgress.tsx
'use client';

import { useState, useEffect } from 'react';

interface ExportProgressProps {
  projectId: string;
  isExporting: boolean;
}

export function ExportProgress({ projectId, isExporting }: ExportProgressProps) {
  const [progress, setProgress] = useState(0);

  useEffect(() => {
    if (!isExporting) {
      setProgress(0);
      return;
    }

    const interval = setInterval(() => {
      setProgress((prev) => {
        if (prev >= 90) return prev;
        return prev + Math.random() * 30;
      });
    }, 500);

    return () => clearInterval(interval);
  }, [isExporting]);

  if (!isExporting) return null;

  return (
    <div className="w-full">
      <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
        <div
          className="h-full bg-blue-500 transition-all"
          style={{ width: `${Math.min(progress, 100)}%` }}
        />
      </div>
      <p className="text-sm text-gray-600 mt-2">
        Generating {Math.round(progress)}%...
      </p>
    </div>
  );
}
```

## 7. Environment Variables

Ensure `.env.local` has:

```env
NEXT_PUBLIC_API_BASE=http://localhost:5500/api
```

## 8. TypeScript Types

Create `types/export.ts`:

```typescript
export interface ExportResult {
  exportId: string;
  projectId: string;
  framework: 'html' | 'react' | 'nextjs';
  downloadUrl: string;
  fileSize: number;
  createdAt: string;
}

export type Framework = 'html' | 'react' | 'nextjs';

export interface ExportError {
  message: string;
  code?: string;
}
```

## 9. Error Handling

Comprehensive error handling:

```typescript
async function downloadCode(projectId: string, framework: Framework) {
  try {
    const response = await fetch(
      `${process.env.NEXT_PUBLIC_API_BASE}/export/${projectId}/${framework}`,
      { method: 'POST' }
    );

    if (response.status === 404) {
      throw new Error('Project not found');
    }
    if (response.status === 400) {
      throw new Error('Invalid request - check project ID and framework');
    }
    if (response.status === 500) {
      throw new Error('Server error - please try again later');
    }
    if (!response.ok) {
      throw new Error(`Export failed: ${response.statusText}`);
    }

    const result = await response.json();
    // Proceed with download...
  } catch (error) {
    console.error('Export failed:', error);
    // Show user-friendly error message
  }
}
```

## 10. Testing

Test export functionality:

```typescript
// __tests__/export.test.ts
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ExportButtons } from '@/components/export/ExportButtons';

describe('ExportButtons', () => {
  it('should download HTML export', async () => {
    render(
      <ExportButtons
        projectId="test-123"
        projectName="Test Project"
      />
    );

    const htmlButton = screen.getByText(/HTML/i);
    fireEvent.click(htmlButton);

    await waitFor(() => {
      expect(screen.getByText(/generated/i)).toBeInTheDocument();
    });
  });
});
```

## 11. Accessibility

Ensure keyboard navigation:

```typescript
<Button
  onClick={() => handleDownload(fw.value)}
  disabled={isExporting}
  aria-label={`Download project as ${fw.label}`}
  aria-busy={isExporting}
>
  {fw.label}
</Button>
```

## 12. Next Steps

- [ ] Integrate Export Service with Gateway
- [ ] Add download buttons to project dashboard
- [ ] Test all three frameworks (HTML, React, Next.js)
- [ ] Verify file downloads work
- [ ] Monitor export service logs
- [ ] Gather user feedback on exported code quality

## Troubleshooting

### Download doesn't start
- Check browser console for errors
- Verify API_BASE is correct
- Test endpoint directly: `curl http://localhost:5500/api/export/frameworks`

### "CORS error"
- Verify export service has CORS enabled
- Check gateway CORS configuration
- Frontend origin (localhost:3000) must be allowed

### Export takes too long
- GeneratorService might be slow
- Large project with many components
- Check server logs for bottlenecks
- Consider caching exports

### Downloaded file is corrupted
- Check file wasn't truncated during transfer
- Verify zip generation logic
- Try downloading same project again

## Performance Tips

1. **Cache exports** for unchanged projects
2. **Show progress** for large projects
3. **Implement retry** for failed downloads
4. **Queue exports** if high load
5. **Compress** generated code when possible
