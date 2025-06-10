import { useTheme } from "@/components/theme-provider";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { Settings, Palette, Monitor, Sun, Moon } from "lucide-react";

export default function SettingsPage() {
  const { theme, setTheme } = useTheme();

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-2">
        <Settings className="w-5 h-5" />
        <h1 className="text-2xl font-bold">Settings</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center space-x-2">
            <Palette className="w-5 h-5" />
            <span>Appearance</span>
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="space-y-3">
            <Label htmlFor="theme">Theme</Label>
            <Select value={theme} onValueChange={setTheme}>
              <SelectTrigger className="w-full sm:w-[200px]">
                <SelectValue placeholder="Select theme" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="light">
                  <div className="flex items-center space-x-2">
                    <Sun className="w-4 h-4" />
                    <span>Light</span>
                  </div>
                </SelectItem>
                <SelectItem value="dark">
                  <div className="flex items-center space-x-2">
                    <Moon className="w-4 h-4" />
                    <span>Dark</span>
                  </div>
                </SelectItem>
                <SelectItem value="system">
                  <div className="flex items-center space-x-2">
                    <Monitor className="w-4 h-4" />
                    <span>System</span>
                  </div>
                </SelectItem>
              </SelectContent>
            </Select>
            <p className="text-sm text-muted-foreground">
              Choose how the application looks. Select "System" to use your device's theme preference.
            </p>
          </div>

          <Separator />

          <div className="space-y-3">
            <Label>Theme Preview</Label>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <Button
                variant={theme === "light" ? "default" : "outline"}
                className="h-20 flex flex-col items-center justify-center space-y-2"
                onClick={() => setTheme("light")}
              >
                <Sun className="w-6 h-6" />
                <span>Light</span>
              </Button>
              <Button
                variant={theme === "dark" ? "default" : "outline"}
                className="h-20 flex flex-col items-center justify-center space-y-2"
                onClick={() => setTheme("dark")}
              >
                <Moon className="w-6 h-6" />
                <span>Dark</span>
              </Button>
              <Button
                variant={theme === "system" ? "default" : "outline"}
                className="h-20 flex flex-col items-center justify-center space-y-2"
                onClick={() => setTheme("system")}
              >
                <Monitor className="w-6 h-6" />
                <span>System</span>
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Data Management</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-3">
            <Label>Storage</Label>
            <p className="text-sm text-muted-foreground">
              Person data is stored locally in memory. Data will be reset when the application restarts.
            </p>
          </div>
          
          <Separator />
          
          <div className="space-y-3">
            <Label>API Endpoints</Label>
            <div className="text-sm text-muted-foreground space-y-1">
              <p>• POST /person - Create new person</p>
              <p>• GET /person/list - List persons with pagination</p>
              <p>• GET /person/search - Search persons</p>
              <p>• GET /person/count - Get total count</p>
              <p>• PUT /person/:id - Update person</p>
              <p>• DELETE /person/:id - Delete person</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}