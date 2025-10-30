'use client';

import * as React from 'react';
import { Control, Controller, FieldPath, FieldValues } from 'react-hook-form';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';

interface FormInputProps<TFieldValues extends FieldValues = any> {
  control: Control<TFieldValues>;
  name: FieldPath<TFieldValues>;
  label?: string;
  placeholder?: string;
  type?: string;
  icon?: React.ReactNode;
  error?: string;
  helperText?: string;
  disabled?: boolean;
  required?: boolean;
}

export const FormInput = React.forwardRef<
  HTMLInputElement,
  FormInputProps
>(
  (
    {
      control,
      name,
      label,
      placeholder,
      type = 'text',
      icon,
      error,
      helperText,
      disabled,
      required,
    },
    ref
  ) => {
    return (
      <Controller
        control={control}
        name={name}
        render={({ field, fieldState: { error: fieldError } }) => (
          <div className="w-full">
            {label && (
              <label className="block text-sm font-medium text-gray-700 mb-2">
                {label}
                {required && <span className="text-red-500 ml-1">*</span>}
              </label>
            )}
            <div className="relative">
              {icon && (
                <div className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">
                  {icon}
                </div>
              )}
              <Input
                type={type}
                placeholder={placeholder}
                disabled={disabled}
                className={cn(
                  'w-full',
                  icon && 'pl-10',
                  fieldError && 'border-red-500 focus-visible:ring-red-500'
                )}
                {...field}
                value={field.value || ''}
              />
            </div>
            {fieldError && (
              <p className="mt-1 text-sm text-red-500">
                {fieldError.message}
              </p>
            )}
            {helperText && !fieldError && (
              <p className="mt-1 text-sm text-gray-500">{helperText}</p>
            )}
          </div>
        )}
      />
    );
  }
);

FormInput.displayName = 'FormInput';
