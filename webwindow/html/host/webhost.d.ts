/// <reference types="node" />
import http = require("http");
import electron = require('electron');
export declare class wshost {
    server: http.Server | null;
    port: number;
    windows: {
        [id: number]: electron.BrowserWindow;
    };
    constructor();
    begin(url: string): void;
    onRequest(req: http.IncomingMessage, res: http.ServerResponse): void;
    onMethod(method: string, params: any, res: http.ServerResponse): boolean;
    regWindow(window: electron.BrowserWindow): number;
    closeWindow(id: number): boolean;
    showWindow(id: number, show: boolean, focus?: boolean): boolean;
}
