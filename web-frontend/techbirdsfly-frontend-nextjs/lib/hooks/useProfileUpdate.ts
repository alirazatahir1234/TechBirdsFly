import { useState } from "react";
import toast from "react-hot-toast";
import { useAuthStore } from "@/lib/store/authStore";

const API_BASE = process.env.NEXT_PUBLIC_API_BASE || "http://localhost:5500/api";

export interface ProfileUpdateData {
  firstName?: string;
  lastName?: string;
  email?: string;
}

export interface PasswordChangeData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export const useProfileUpdate = () => {
  const [isLoading, setIsLoading] = useState(false);
  const { user, updateUser } = useAuthStore();

  const updateProfile = async (data: ProfileUpdateData) => {
    if (!user?.id) {
      toast.error("User not authenticated");
      return false;
    }

    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE}/users/${user.id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || "Failed to update profile");
      }

      const updatedUser = await response.json();
      updateUser(updatedUser);
      toast.success("Profile updated successfully!");
      return true;
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to update profile";
      toast.error(message);
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  const changePassword = async (data: PasswordChangeData) => {
    if (data.newPassword !== data.confirmPassword) {
      toast.error("Passwords do not match");
      return false;
    }

    if (!user?.id) {
      toast.error("User not authenticated");
      return false;
    }

    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE}/users/${user.id}/change-password`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          currentPassword: data.currentPassword,
          newPassword: data.newPassword,
        }),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || "Failed to change password");
      }

      toast.success("Password changed successfully!");
      return true;
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to change password";
      toast.error(message);
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  return {
    isLoading,
    updateProfile,
    changePassword,
  };
};
