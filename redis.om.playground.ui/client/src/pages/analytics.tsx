// import React from "react";
// import { useSubscription, gql } from "@apollo/client";
// import { BarChart3 } from "lucide-react";
// import { Card, CardContent } from "@/components/ui/card";
// import {
//   LineChart,
//   Line,
//   XAxis,
//   YAxis,
//   Tooltip,
//   CartesianGrid,
//   ResponsiveContainer,
// } from "recharts";

// // Type definitions
// interface DataPoint {
//   timestamp: number;
//   value: number;
// }

// interface UserCreatedData {
//   values: DataPoint[];
// }

// interface SubscriptionData {
//   userCreated: UserCreatedData;
// }

// interface ChartDataPoint extends DataPoint {
//   time: string;
// }

// // GraphQL subscription
// const USER_CREATED_SUBSCRIPTION = gql`
//   subscription {
//     userCreated {
//       values {
//         timestamp
//         value
//       }
//     }
//   }
// `;

// export default function Analytics() {


//   // const { data, loading, error } = useSubscription<SubscriptionData>(USER_CREATED_SUBSCRIPTION);

//   // if (loading) return <div>Loading...</div>;
//   // if (error) return <div>Error: {error.message}</div>;

//   const data: SubscriptionData = {
//     userCreated: {
//       values: [
//         { timestamp: 1750256460000, value: 10 },
//         { timestamp: 1750256465000, value: 20 },
//         { timestamp: 1750256470000, value: 30 },
//       ],
//     },
//   }; // Mock data for demonstration


//   // Extract and format data for the chart
//   const chartData: ChartDataPoint[] =
//     data?.userCreated?.values?.map((item: DataPoint) => ({
//       ...item,
//       // Format timestamp for display
//       time: new Date(item.timestamp).toLocaleTimeString(),
//     })) || [];

//   return (

//     <Card>
//       <CardContent className="pt-6">
//         <div className="text-center">
//           <BarChart3 className="w-12 h-12 text-gray-400 mx-auto mb-4" />
//           <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
//             Analytics Dashboard
//           </h3>
//         </div>
//         <ResponsiveContainer width="100%" height={300}>
//           <LineChart data={chartData}>
//             <CartesianGrid stroke="#eee" strokeDasharray="5 5" />
//             <XAxis dataKey="time" />
//             <YAxis />
//             <Tooltip />
//             <Line type="monotone" dataKey="value" stroke="#8884d8" />
//           </LineChart>
//         </ResponsiveContainer>
//       </CardContent>
//     </Card>
//   );
// }

import { Card, CardContent } from '@/components/ui/card';
import { getConfig } from '@/lib/config';
import React, { useState, useEffect, useRef } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

interface DataPoint {
    timestamp: number;
    value: number;
}

interface SubscriptionData {
    data: {
        userCreated: {
            values: DataPoint[];
        };
    };
}

interface WebSocketMessage {
    type: string;
    id?: string;
    payload?: any;
}

interface GraphQLSubscription {
    query: string;
    variables?: Record<string, AggregationType>;
}

type ConnectionStatus = 'Disconnected' | 'Connecting' | 'Connected' | 'Error' | 'Completed';
type AggregationType = 'RAW' | 'ONE_MINUTE' | 'FIVE_MINUTES' | 'FIFTEEN_MINUTES' | 'ONE_HOUR';

const RealtimeChart: React.FC = () => {
    const [data, setData] = useState<DataPoint[]>([]);
    const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('Disconnected');
    const wsRef = useRef<WebSocket | null>(null);

    // GraphQL subscription query
    const subscriptionQuery: GraphQLSubscription = {
        query: `
          subscription($aggregation: AggregationType!) {
            userCreated(aggregation: $aggregation) {
                values {
                    timestamp
                    value
                }
            }
        }`,
        variables: {
            aggregation: 'ONE_MINUTE'
        }
    };

    const connectWebSocket = (url: string): void => {
        try {
            // Close existing connection if any
            if (wsRef.current) {
                wsRef.current.close();
            }

            setConnectionStatus('Connecting');
            const ws = new WebSocket(url, 'graphql-ws');
            wsRef.current = ws;

            ws.onopen = () => {
                setConnectionStatus('Connected');

                // Send connection init message (GraphQL-WS protocol)
                ws.send(JSON.stringify({
                    type: 'connection_init'
                }));
            };

            ws.onmessage = (event: MessageEvent) => {
                try {
                    const message: WebSocketMessage = JSON.parse(event.data);

                    switch (message.type) {
                        case 'connection_ack':
                            // Connection acknowledged, start subscription
                            ws.send(JSON.stringify({
                                id: 'subscription-1',
                                type: 'start',
                                payload: subscriptionQuery
                            }));
                            break;

                        case 'data':
                            // Handle subscription data - replace entire dataset
                            if (message.payload?.data?.userCreated?.values) {
                                const subscriptionData: SubscriptionData = message.payload;
                                const newValues = subscriptionData.data.userCreated.values;

                                // Sort the full dataset by timestamp
                                const sortedData = [...newValues].sort((a, b) => a.timestamp - b.timestamp);

                                // Replace the entire dataset (since subscription returns full data)
                                setData(sortedData);
                            }
                            break;

                        case 'error':
                            console.error('GraphQL subscription error:', message.payload);
                            setConnectionStatus('Error');
                            break;

                        case 'complete':
                            setConnectionStatus('Completed');
                            break;
                    }
                } catch (parseError) {
                    console.error('Failed to parse WebSocket message:', parseError);
                }
            };

            ws.onclose = (event: CloseEvent) => {
                setConnectionStatus('Disconnected');
                console.log('WebSocket closed:', event.reason);
            };

            ws.onerror = (error: Event) => {
                console.error('WebSocket error:', error);
                setConnectionStatus('Error');
            };

        } catch (error) {
            console.error('Failed to connect:', error);
            setConnectionStatus('Error');
        }
    };

    const disconnect = (): void => {
        if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
            wsRef.current.send(JSON.stringify({
                id: 'subscription-1',
                type: 'stop'
            }));

            wsRef.current.close();
            wsRef.current = null;
        }
    };

    useEffect(() => {
        const timer = setTimeout(async () => {
            const config = await getConfig();
            connectWebSocket(config.apiSocketUrl + config.graphQlPath);
        }, 100); // Small delay to avoid React strict mode issues

        return () => {
            clearTimeout(timer);
            disconnect();
        };
    }, []);

    const getStatusColor = (): string => {
        switch (connectionStatus) {
            case 'Connected': return 'text-green-600';
            case 'Connecting': return 'text-yellow-600';
            case 'Error': return 'text-red-600';
            case 'Disconnected': return 'text-gray-600';
            case 'Completed': return 'text-blue-600';
            default: return 'text-gray-600';
        }
    };

    return (

        <Card>
            <CardContent className="pt-6">

                <div className="p-6 max-w-6xl mx-auto bg-white">
                    <h1 className="text-3xl font-bold text-gray-800 mb-6">Real-time GraphQL Subscription Chart</h1>

                    {/* Connection Controls */}
                    <div className="bg-gray-50 p-4 rounded-lg mb-6">
                        <div className="flex flex-col gap-4">

                            <div className="flex items-center gap-4">
                                <span className="font-medium text-gray-700">Status:</span>
                                <span className={`font-semibold ${getStatusColor()}`}>
                                    {connectionStatus}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Chart */}
                    <div className="bg-white border border-gray-200 rounded-lg p-4">
                        <h2 className="text-xl font-semibold text-gray-800 mb-4">
                            Users created ({data.length} points)
                        </h2>

                        {data.length === 0 ? (
                            <div className="h-96 flex items-center justify-center text-gray-500">
                                <div className="text-center">
                                    <p className="text-lg mb-2">No data available</p>
                                    <p className="text-sm">Connect to your GraphQL subscription or simulate data to see the chart</p>
                                </div>
                            </div>
                        ) : (
                            <div className="h-96">
                                <ResponsiveContainer width="100%" height={300}>
                                    <LineChart data={data}>
                                        <CartesianGrid stroke="#eee" strokeDasharray="5 5" />
                                        <XAxis dataKey="time" />
                                        <YAxis />
                                        <Tooltip />
                                        <Line type="monotone" dataKey="value" stroke="#8884d8" />
                                    </LineChart>
                                </ResponsiveContainer>
                            </div>
                        )}
                    </div>

                </div>
            </CardContent>
        </Card>

    );
};

export default RealtimeChart;
