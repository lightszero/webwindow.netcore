/// <reference path="../../vscode_part/webhost/electron.d.ts" />
declare const remote: Electron.Remote;
declare function closethis(): void;
declare function settitle(title: string): void;
declare function decodequery(): {
    curl: string;
    winid: number;
};
declare let __pageoption: {
    curl: string;
    winid: number;
};
declare function testcode(): void;
