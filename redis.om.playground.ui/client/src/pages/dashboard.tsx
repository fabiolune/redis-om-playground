import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { type Person } from "@shared/schema";
import { type ApiResponse } from "@/lib/types";
import { Sidebar } from "@/components/layout/sidebar";
import { Topbar } from "@/components/layout/topbar";
import { PersonList } from "@/components/person-list";
import { PersonForm } from "@/components/person-form";
import SettingsPage from "@/pages/settings";
import SearchPage from "@/pages/search";
import EditPersonPage from "@/pages/edit-person";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { 
  Users, 
  UserPlus, 
  Search, 
  BarChart3,
  Database,
  Share2,
  Plus,
  TrendingUp
} from "lucide-react";
import { useLocation, Route, Switch } from "wouter";
import { apiRequest } from "@/lib/queryClient";

export default function Dashboard() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [location] = useLocation();

  // Fetch person count - only when dashboard is active
  const { data: countData } = useQuery<ApiResponse<number>>({
    queryKey: ["/person/count"],
    queryFn: async () => {
      const response = await apiRequest("GET", "/person/count");
      
      if (!response.ok) {
        throw new Error(`${response.status}: ${response.statusText}`);
      }
      
      return response.json();
    },
    enabled: location === "/", // Only fetch when on dashboard
  });

  const totalPersons = countData?.data || 0;

  const StatsCards = () => (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 mb-8">
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Total Persons</p>
              <p className="text-2xl font-bold text-gray-900 dark:text-gray-100">{totalPersons}</p>
            </div>
            <div className="p-3 bg-indigo-100 dark:bg-indigo-900 rounded-lg">
              <Users className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
            </div>
          </div>
          <div className="mt-4 flex items-center text-sm">
            <TrendingUp className="w-4 h-4 text-green-600 dark:text-green-400 mr-1" />
            <span className="text-green-600 dark:text-green-400 font-medium">Active</span>
            <span className="text-gray-600 dark:text-gray-400 ml-2">records</span>
          </div>
        </CardContent>
      </Card>
      
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 dark:text-gray-400">API Endpoints</p>
              <p className="text-2xl font-bold text-gray-900 dark:text-gray-100">6</p>
            </div>
            <div className="p-3 bg-cyan-100 dark:bg-cyan-900 rounded-lg">
              <Database className="w-5 h-5 text-cyan-600 dark:text-cyan-400" />
            </div>
          </div>
          <div className="mt-4 flex items-center text-sm">
            <span className="text-blue-600 dark:text-blue-400 font-medium">REST API</span>
            <span className="text-gray-600 dark:text-gray-400 ml-2">available</span>
          </div>
        </CardContent>
      </Card>
      
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Quick Actions</p>
              <p className="text-2xl font-bold text-gray-900 dark:text-gray-100">3</p>
            </div>
            <div className="p-3 bg-emerald-100 dark:bg-emerald-900 rounded-lg">
              <Share2 className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
            </div>
          </div>
          <div className="mt-4 flex items-center text-sm">
            <span className="text-green-600 dark:text-green-400 font-medium">Available</span>
            <span className="text-gray-600 dark:text-gray-400 ml-2">tools</span>
          </div>
        </CardContent>
      </Card>
    </div>
  );

  const QuickActions = () => (
    <Card className="mb-8">
      <CardHeader>
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between">
          <CardTitle>Quick Actions</CardTitle>
          <Button onClick={() => window.location.href = "/persons/new"}>
            <Plus className="w-4 h-4 mr-2" />
            New Person
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <Button
            variant="outline"
            className="h-auto p-4 sm:p-6 flex flex-col sm:flex-row items-center sm:items-start text-left space-y-2 sm:space-y-0 sm:space-x-4"
            onClick={() => window.location.href = "/persons/new"}
          >
            <div className="p-3 bg-blue-100 dark:bg-blue-900 rounded-lg">
              <UserPlus className="w-5 h-5 text-blue-600 dark:text-blue-400" />
            </div>
            <div className="text-center sm:text-left">
              <h4 className="font-semibold text-gray-900 dark:text-gray-100">Add Person</h4>
              <p className="text-sm text-gray-600 dark:text-gray-400 hidden sm:block">Create a new person record</p>
            </div>
          </Button>
          
          <Button
            variant="outline"
            className="h-auto p-4 sm:p-6 flex flex-col sm:flex-row items-center sm:items-start text-left space-y-2 sm:space-y-0 sm:space-x-4"
            onClick={() => window.location.href = "/search"}
          >
            <div className="p-3 bg-green-100 dark:bg-green-900 rounded-lg">
              <Search className="w-5 h-5 text-green-600 dark:text-green-400" />
            </div>
            <div className="text-center sm:text-left">
              <h4 className="font-semibold text-gray-900 dark:text-gray-100">Search & Filter</h4>
              <p className="text-sm text-gray-600 dark:text-gray-400 hidden sm:block">Find specific person records</p>
            </div>
          </Button>
          
          <Button
            variant="outline"
            className="h-auto p-4 sm:p-6 flex flex-col sm:flex-row items-center sm:items-start text-left space-y-2 sm:space-y-0 sm:space-x-4"
            onClick={() => window.location.href = "/analytics"}
          >
            <div className="p-3 bg-purple-100 dark:bg-purple-900 rounded-lg">
              <BarChart3 className="w-5 h-5 text-purple-600 dark:text-purple-400" />
            </div>
            <div className="text-center sm:text-left">
              <h4 className="font-semibold text-gray-900 dark:text-gray-100">View Analytics</h4>
              <p className="text-sm text-gray-600 dark:text-gray-400 hidden sm:block">Analyze person data trends</p>
            </div>
          </Button>
        </div>
      </CardContent>
    </Card>
  );

  return (
    <div className="min-h-screen flex bg-gray-50 dark:bg-gray-900">
      <Sidebar 
        isOpen={sidebarOpen} 
        onClose={() => setSidebarOpen(false)} 
      />
      
      <div className="flex-1 flex flex-col overflow-hidden">
        <Topbar
          onMenuClick={() => setSidebarOpen(true)}
          title="Person Data Management"
          subtitle="Manage and analyze person data with ease"
        />
        
        <main className="flex-1 overflow-y-auto bg-gray-50 dark:bg-gray-900">
          <div className="p-4 sm:p-6 lg:p-8">
            <Switch>
              <Route path="/">
                <StatsCards />
                <QuickActions />
                <PersonList />
              </Route>
              
              <Route path="/persons/new">
                <PersonForm />
              </Route>
              
              <Route path="/persons/:id/edit">
                {(params) => {
                  return <EditPersonPage personId={params.id} />;
                }}
              </Route>
              
              <Route path="/persons">
                <PersonList />
              </Route>
              
              <Route path="/search">
                <SearchPage />
              </Route>
              
              <Route path="/settings">
                <SettingsPage />
              </Route>
              
              <Route path="/analytics">
                <Card>
                  <CardContent className="pt-6">
                    <div className="text-center">
                      <BarChart3 className="w-12 h-12 text-gray-400 mx-auto mb-4" />
                      <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
                        Analytics Dashboard
                      </h3>
                      <p className="text-gray-500 dark:text-gray-400">
                        Advanced analytics features are coming soon.
                      </p>
                    </div>
                  </CardContent>
                </Card>
              </Route>
              
              <Route>
                <Card>
                  <CardContent className="pt-6">
                    <div className="text-center">
                      <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
                        Page Not Found
                      </h3>
                      <p className="text-gray-500 dark:text-gray-400">
                        The page you're looking for doesn't exist.
                      </p>
                    </div>
                  </CardContent>
                </Card>
              </Route>
            </Switch>
          </div>
        </main>
      </div>
      
      {/* Mobile FAB */}
      <div className="fixed bottom-6 right-6 lg:hidden">
        <Button
          size="lg"
          className="w-14 h-14 rounded-full shadow-lg"
          onClick={() => window.location.href = "/persons/new"}
        >
          <Plus className="w-6 h-6" />
        </Button>
      </div>
    </div>
  );
}
