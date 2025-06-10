import { useQuery } from "@tanstack/react-query";
import { type Person } from "@shared/schema";
import { PersonForm } from "@/components/person-form";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { AlertCircle } from "lucide-react";

interface EditPersonPageProps {
  personId: string;
}

export default function EditPersonPage({ personId }: EditPersonPageProps) {
  // Fetch person data for editing
  const { data: person, isLoading, error } = useQuery<Person>({
    queryKey: [`/person/${personId}`],
    enabled: !!personId, // Only fetch if personId exists
  });

  if (isLoading) {
    return (
      <Card className="w-full max-w-2xl mx-auto">
        <CardContent className="pt-6">
          <div className="space-y-6">
            <div className="space-y-2">
              <Skeleton className="h-4 w-1/4" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Skeleton className="h-4 w-1/3" />
                <Skeleton className="h-10 w-full" />
              </div>
              <div className="space-y-2">
                <Skeleton className="h-4 w-1/3" />
                <Skeleton className="h-10 w-full" />
              </div>
            </div>
            <div className="space-y-2">
              <Skeleton className="h-4 w-1/4" />
              <Skeleton className="h-32 w-full" />
            </div>
            <div className="flex justify-end space-x-4">
              <Skeleton className="h-10 w-20" />
              <Skeleton className="h-10 w-32" />
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card className="w-full max-w-2xl mx-auto">
        <CardContent className="pt-6">
          <div className="text-center py-8">
            <AlertCircle className="w-12 h-12 text-red-500 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
              Error Loading Person
            </h3>
            <p className="text-red-600 dark:text-red-400 mb-4">
              {(error as Error).message}
            </p>
            <p className="text-gray-500 dark:text-gray-400">
              The person you're trying to edit could not be found or there was an error loading the data.
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!person) {
    return (
      <Card className="w-full max-w-2xl mx-auto">
        <CardContent className="pt-6">
          <div className="text-center py-8">
            <AlertCircle className="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
              Person Not Found
            </h3>
            <p className="text-gray-500 dark:text-gray-400">
              The person with ID {personId} was not found.
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return <PersonForm person={person} />;
}