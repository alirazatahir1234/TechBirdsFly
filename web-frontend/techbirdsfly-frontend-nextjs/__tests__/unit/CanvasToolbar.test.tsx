import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders, userEvent } from '../utils/test-utils';
import { CanvasToolbar } from '@/components/builder/CanvasToolbar';
import { useBuilderStore } from '@/lib/store/builderStore';

// Mock the store
vi.mock('@/lib/store/builderStore');

describe('CanvasToolbar', () => {
  const mockTransform = {
    x: 100,
    y: 50,
    scale: 1.5,
    rotation: 45,
  };

  const defaultProps = {
    canvasTransform: mockTransform,
    onResetZoom: vi.fn(),
    onZoomIn: vi.fn(),
    onZoomOut: vi.fn(),
  };

  beforeEach(() => {
    vi.mocked(useBuilderStore).mockReturnValue({
      togglePreviewMode: vi.fn(),
      isPreviewMode: false,
    } as any);
  });

  it('should render zoom controls', () => {
    renderWithProviders(<CanvasToolbar {...defaultProps} />);

    expect(screen.getByTitle(/zoom out/i)).toBeInTheDocument();
    expect(screen.getByTitle(/zoom in/i)).toBeInTheDocument();
    expect(screen.getByTitle(/reset zoom/i)).toBeInTheDocument();
  });

  it('should display current zoom level', () => {
    renderWithProviders(<CanvasToolbar {...defaultProps} />);

    expect(screen.getByText('150%')).toBeInTheDocument();
  });

  it('should display position and rotation', () => {
    renderWithProviders(<CanvasToolbar {...defaultProps} />);

    expect(screen.getByText(/Position: \(100, 50\)/)).toBeInTheDocument();
    expect(screen.getByText(/Rotation: 45°/)).toBeInTheDocument();
  });

  it('should call onZoomIn when zoom in button is clicked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<CanvasToolbar {...defaultProps} />);

    const zoomInBtn = screen.getByTitle(/zoom in/i);
    await user.click(zoomInBtn);

    expect(defaultProps.onZoomIn).toHaveBeenCalledTimes(1);
  });

  it('should call onZoomOut when zoom out button is clicked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<CanvasToolbar {...defaultProps} />);

    const zoomOutBtn = screen.getByTitle(/zoom out/i);
    await user.click(zoomOutBtn);

    expect(defaultProps.onZoomOut).toHaveBeenCalledTimes(1);
  });

  it('should call onResetZoom when reset button is clicked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<CanvasToolbar {...defaultProps} />);

    const resetBtn = screen.getByTitle(/reset zoom/i);
    await user.click(resetBtn);

    expect(defaultProps.onResetZoom).toHaveBeenCalledTimes(1);
  });

  it('should toggle preview mode', async () => {
    const user = userEvent.setup();
    const mockToggle = vi.fn();
    
    vi.mocked(useBuilderStore).mockReturnValue({
      togglePreviewMode: mockToggle,
      isPreviewMode: false,
    } as any);

    renderWithProviders(<CanvasToolbar {...defaultProps} />);

    const previewBtn = screen.getByTitle(/toggle preview/i);
    await user.click(previewBtn);

    expect(mockToggle).toHaveBeenCalledTimes(1);
  });

  it('should show different button state in preview mode', () => {
    vi.mocked(useBuilderStore).mockReturnValue({
      togglePreviewMode: vi.fn(),
      isPreviewMode: true,
    } as any);

    renderWithProviders(<CanvasToolbar {...defaultProps} />);

    expect(screen.getByText('Preview')).toBeInTheDocument();
  });
});
