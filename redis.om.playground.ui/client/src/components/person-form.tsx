import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from "@/hooks/use-toast";
import { apiRequest } from "@/lib/queryClient";
import { zodResolver } from "@hookform/resolvers/zod";
import { insertPersonSchema, type InsertPerson, type Person } from "@shared/schema";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Loader2, Save } from "lucide-react";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { useLocation } from "wouter";

interface PersonFormProps {
    person?: Person;
    onSuccess?: () => void;
}

export function PersonForm({ person, onSuccess }: PersonFormProps) {
    const [, setLocation] = useLocation();
    const { toast } = useToast();
    const queryClient = useQueryClient();

    const form = useForm<InsertPerson>({
        resolver: zodResolver(insertPersonSchema),
        defaultValues: {
            firstName: person?.firstName || "",
            lastName: person?.lastName || "",
            personalStatement: person?.personalStatement || "",
        },
    });

    // Reset form when person data changes (for edit mode)
    useEffect(() => {
        if (person) {
            form.reset({
                firstName: person.firstName,
                lastName: person.lastName,
                personalStatement: person.personalStatement,
            });
        }
    }, [person, form]);

    const createMutation = useMutation({
        mutationFn: async (data: InsertPerson) => {
            const response = await apiRequest("POST", "/person", data);
            return response.json();
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["/person/list"] });
            queryClient.invalidateQueries({ queryKey: ["/person/count"] });
            toast({
                title: "Success",
                description: "Person created successfully",
            });
            if (onSuccess) {
                onSuccess();
            } else {
                setLocation("/");
            }
        },
        onError: (error: any) => {
            toast({
                title: "Error",
                description: error.message || "Failed to create person",
                variant: "destructive",
            });
        },
    });

    const updateMutation = useMutation({
        mutationFn: async (data: InsertPerson) => {
            const response = await apiRequest("PUT", `/person/${person!.id}`, data);
            return response.json();
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["/person/list"] });
            queryClient.invalidateQueries({ queryKey: ["/person/search"] });
            queryClient.invalidateQueries({ queryKey: ["/person", person!.id] });
            queryClient.invalidateQueries({ queryKey: ["/person/count"] });
            toast({
                title: "Success",
                description: "Person updated successfully",
            });
            if (onSuccess) {
                onSuccess();
            } else {
                setLocation("/");
            }
        },
        onError: (error: any) => {
            toast({
                title: "Error",
                description: error.message || "Failed to update person",
                variant: "destructive",
            });
        },
    });

    const onSubmit = (data: InsertPerson) => {
        if (person) {
            updateMutation.mutate(data);
        } else {
            createMutation.mutate(data);
        }
    };

    const isLoading = createMutation.isPending || updateMutation.isPending;

    return (
        <Card className="w-full max-w-2xl mx-auto">
            <CardHeader>
                <CardTitle className="flex items-center space-x-2">
                    <Save className="w-5 h-5" />
                    <span>{person ? "Edit Person" : "Add New Person"}</span>
                </CardTitle>
            </CardHeader>
            <CardContent>
                <Form {...form}>
                    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <FormField
                                control={form.control}
                                name="firstName"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>First Name</FormLabel>
                                        <FormControl>
                                            <Input placeholder="Enter first name" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />

                            <FormField
                                control={form.control}
                                name="lastName"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Last Name</FormLabel>
                                        <FormControl>
                                            <Input placeholder="Enter last name" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>

                        <FormField
                            control={form.control}
                            name="personalStatement"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Personal Statement</FormLabel>
                                    <FormControl>
                                        <Textarea
                                            placeholder="Enter personal statement"
                                            className="min-h-[120px]"
                                            {...field}
                                        />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        <div className="flex justify-end space-x-4">
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => setLocation("/")}
                                disabled={isLoading}
                            >
                                Cancel
                            </Button>
                            <Button type="submit" disabled={isLoading}>
                                {isLoading && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
                                {person ? "Update Person" : "Create Person"}
                            </Button>
                        </div>
                    </form>
                </Form>
            </CardContent>
        </Card>
    );
}
