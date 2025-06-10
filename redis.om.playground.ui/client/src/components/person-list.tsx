import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { useToast } from "@/hooks/use-toast";
import { apiRequest } from "@/lib/queryClient";
import { type ApiResponse } from "@/lib/types";
import { type Person } from "@shared/schema";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    ChevronLeft,
    ChevronRight,
    Edit2,
    Plus,
    Search,
    Trash2,
    Users
} from "lucide-react";
import { useState } from "react";
import { useLocation } from "wouter";

export function PersonList() {
    console.log("Rendering PersonList");
    const [, setLocation] = useLocation();
    const [searchQuery, setSearchQuery] = useState("");
    const [currentPage, setCurrentPage] = useState(1);
    const { toast } = useToast();
    const queryClient = useQueryClient();

    const limit = 10;

    // Fetch persons with pagination - refetch when component mounts
    const { data, isLoading, error } = useQuery<ApiResponse<Person[]>>({
        queryKey: searchQuery
            ? ["/person/search", { q: searchQuery, page: currentPage, limit }]
            : ["/person/list", { page: currentPage, limit }],
        staleTime: 500, // Always refetch when component mounts
        refetchOnMount: true,
    });

    // Delete person mutation
    const deleteMutation = useMutation({
        mutationFn: async (id: number) => {
            await apiRequest("DELETE", `/person/${id}`);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["/person/list"] });
            queryClient.invalidateQueries({ queryKey: ["/person/search"] });
            queryClient.invalidateQueries({ queryKey: ["/person/count"] });
            toast({
                title: "Success",
                description: "Person deleted successfully",
            });
        },
        onError: (error: any) => {
            toast({
                title: "Error",
                description: error.message || "Failed to delete person",
                variant: "destructive",
            });
        },
    });

    const handleSearch = (query: string) => {
        setSearchQuery(query);
        setCurrentPage(1);
    };

    const handleDelete = (id: number, name: string) => {
        if (confirm(`Are you sure you want to delete ${name}?`)) {
            deleteMutation.mutate(id);
        }
    };

    const handlePageChange = (page: number) => {
        setCurrentPage(page);
    };

    if (error) {
        return (
            <Card>
                <CardContent className="pt-6">
                    <div className="text-center text-red-600">
                        Error loading persons: {(error as Error).message}
                    </div>
                </CardContent>
            </Card>
        );
    }

    const persons = data?.data || [];
    const pagination = data?.pagination;

    return (
        <div className="space-y-6">
            {/* Header and Search */}
            <Card>
                <CardHeader>
                    <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                        <CardTitle className="flex items-center space-x-2">
                            <Users className="w-5 h-5" />
                            <span>Person Management</span>
                        </CardTitle>
                        <Button onClick={() => setLocation("/persons/new")}>
                            <Plus className="w-4 h-4 mr-2" />
                            Add Person
                        </Button>
                    </div>
                </CardHeader>
                <CardContent>
                    <div className="relative">
                        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
                        <Input
                            placeholder="Search by name or personal statement..."
                            value={searchQuery}
                            onChange={(e) => handleSearch(e.target.value)}
                            className="pl-10"
                        />
                    </div>
                </CardContent>
            </Card>
            {/* Results */}
            <Card>
                <CardContent className="pt-6">
                    {isLoading ? (
                        <div className="space-y-4">
                            {Array.from({ length: 5 }).map((_, i) => (
                                <div key={i} className="flex items-center justify-between p-4 border rounded-lg">
                                    <div className="space-y-2 flex-1">
                                        <Skeleton className="h-4 w-1/3" />
                                        <Skeleton className="h-3 w-2/3" />
                                    </div>
                                    <div className="flex space-x-2">
                                        <Skeleton className="h-8 w-16" />
                                        <Skeleton className="h-8 w-16" />
                                    </div>
                                </div>
                            ))}
                        </div>
                    ) : persons.length === 0 ? (
                        <div className="text-center py-12">
                            <Users className="w-12 h-12 text-gray-400 dark:text-gray-500 mx-auto mb-4" />
                            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
                                {searchQuery ? "No results found" : "No persons yet"}
                            </h3>
                            <p className="text-gray-500 dark:text-gray-400 mb-4">
                                {searchQuery
                                    ? "Try adjusting your search terms"
                                    : "Get started by adding your first person"
                                }
                            </p>
                            {!searchQuery && (
                                <Button onClick={() => setLocation("/persons/new")}>
                                    <Plus className="w-4 h-4 mr-2" />
                                    Add First Person
                                </Button>
                            )}
                        </div>
                    ) : (
                        <div className="space-y-4">
                            {persons.map((person) => (
                                <div
                                    key={person.id}
                                    className="flex items-start justify-between p-4 border rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                                >
                                    <div className="flex-1">
                                        <div className="flex items-center space-x-3 mb-2">
                                            <h3 className="font-semibold text-gray-900 dark:text-gray-100">
                                                {person.firstName} {person.lastName}
                                            </h3>
                                            <Badge variant="secondary">ID: {person.id}</Badge>
                                        </div>
                                        <p className="text-gray-600 dark:text-gray-300 text-sm line-clamp-2">
                                            {person.personalStatement}
                                        </p>
                                    </div>

                                    <div className="flex items-center space-x-2 ml-4">
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setLocation(`/persons/${person.id}/edit`)}
                                        >
                                            <Edit2 className="w-4 h-4" />
                                        </Button>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => handleDelete(person.id, `${person.firstName} ${person.lastName}`)}
                                            disabled={deleteMutation.isPending}
                                        >
                                            <Trash2 className="w-4 h-4" />
                                        </Button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}

                    {/* Pagination */}
                    {pagination && pagination.totalPages > 1 && (
                        <div className="flex items-center justify-between mt-6 pt-6 border-t">
                            <div className="text-sm text-gray-600 dark:text-gray-300">
                                Showing {(pagination.page - 1) * pagination.limit + 1} to{" "}
                                {Math.min(pagination.page * pagination.limit, pagination.total)} of{" "}
                                {pagination.total} results
                            </div>

                            <div className="flex items-center space-x-2">
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => handlePageChange(pagination.page - 1)}
                                    disabled={pagination.page <= 1}
                                >
                                    <ChevronLeft className="w-4 h-4" />
                                    Previous
                                </Button>

                                <div className="flex items-center space-x-1">
                                    {Array.from({ length: Math.min(5, pagination.totalPages) }, (_, i) => {
                                        const page = i + 1;
                                        return (
                                            <Button
                                                key={page}
                                                variant={page === pagination.page ? "default" : "outline"}
                                                size="sm"
                                                onClick={() => handlePageChange(page)}
                                            >
                                                {page}
                                            </Button>
                                        );
                                    })}
                                </div>

                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => handlePageChange(pagination.page + 1)}
                                    disabled={pagination.page >= pagination.totalPages}
                                >
                                    Next
                                    <ChevronRight className="w-4 h-4" />
                                </Button>
                            </div>
                        </div>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
