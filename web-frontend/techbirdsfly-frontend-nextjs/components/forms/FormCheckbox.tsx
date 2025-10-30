'use client';

import * as React from 'react';
import { Control, Controller, FieldPath, FieldValues } from 'react-hook-form';
import { cn } from '@/lib/utils';

interface FormCheckboxProps<TFieldValues extends FieldValues = any> {
  control: Control<TFieldValues>;
  name: FieldPath<TFieldValues>;
  label?: string | React.ReactNode;
  description?: string;
  error?: string;
  disabled?: boolean;
}

export const FormCheckbox = React.forwardRef<
  HTMLInputElement,
  FormCheckboxProps
>(
  (
    { control, name, label, description, error, disabled },
    ref
  ) => {
    return (
      <Controller
        control={control}
        name={name}
        render={({ field, fieldState: { error: fieldError } }) => (
          <div className="flex items-start gap-3 w-full">
            <input
              type="checkbox"
              className={cn(
                'mt-1 rounded border-gray-300 text-purple-600 focus:ring-purple-500 cursor-pointer',
                fieldError && 'border-red-500'
              )}
              disabled={disabled}
              {...field}
              checked={field.value || false}
            />
            <div className="flex-1">
              {label && (
                <label className="text-sm font-medium text-gray-700 cursor-pointer">
                  {label}
                </label>
              )}
              {description && (
                <p className="text-xs text-gray-500 mt-1">{description}</p>
              )}
              {fieldError && (
                <p className="mt-1 text-xs text-red-500">{fieldError.message}</p>
              )}
            </div>
          </div>
        )}
      />
    );
  }
);

FormCheckbox.displayName = 'FormCheckbox';
