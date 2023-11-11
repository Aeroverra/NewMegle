var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
export var peerConnection;
export var stream;
export var dotnet;
export function setup(dotnetInput) {
    return __awaiter(this, void 0, void 0, function* () {
        dotnet = dotnetInput;
        try {
            stream = yield navigator.mediaDevices.getUserMedia({ video: true, audio: true });
            const videoElement = document.querySelector('video#localVideo');
            videoElement.srcObject = stream;
            videoElement.autoplay = true;
            videoElement.play().catch(console.error);
        }
        catch (error) {
            console.error('Error accessing media devices.', error);
        }
    });
}
export function setupPeerConnection(connectionId) {
    return __awaiter(this, void 0, void 0, function* () {
        peerConnection = new RTCPeerConnection();
        peerConnection.oniceconnectionstatechange = (event) => {
            console.log(`ICE state: ${peerConnection.iceConnectionState}`);
            console.log('ICE state change event: ', event);
        };
        peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                dotnet.invokeMethodAsync('SendIce', connectionId, JSON.stringify(event.candidate));
            }
        };
        peerConnection.ontrack = (event) => {
            const [remoteStream] = event.streams;
            const videoElement = document.querySelector('video#remoteVideo');
            videoElement.srcObject = remoteStream;
            videoElement.autoplay = true;
            videoElement.play().catch(console.error);
        };
        stream.getTracks().forEach(track => {
            peerConnection.addTrack(track, stream);
        });
    });
}
export function createOffer(connectionId) {
    return __awaiter(this, void 0, void 0, function* () {
        yield setupPeerConnection(connectionId);
        const offer = yield peerConnection.createOffer();
        yield peerConnection.setLocalDescription(offer);
        var offerString = JSON.stringify(offer);
        return offerString;
    });
}
export function acceptOffer(connectionId, offerString) {
    return __awaiter(this, void 0, void 0, function* () {
        yield setupPeerConnection(connectionId);
        var offer = JSON.parse(offerString);
        yield peerConnection.setRemoteDescription(new RTCSessionDescription(offer));
        const answer = yield peerConnection.createAnswer();
        yield peerConnection.setLocalDescription(answer);
        // Send the answer back to the signaling server to be passed to the share page
        var answerString = JSON.stringify(answer);
        return answerString;
    });
}
export function acceptAnswer(connectionId, answerString) {
    var answerString;
    return __awaiter(this, void 0, void 0, function* () {
        console.log("answer");
        console.log(answerString);
        var answer = JSON.parse(answerString);
        yield peerConnection.setRemoteDescription(new RTCSessionDescription(answer));
        answerString = JSON.stringify(answer);
        return answerString;
    });
}
export function acceptIce(connectionId, iceString) {
    return __awaiter(this, void 0, void 0, function* () {
        console.log("reccc ice");
        console.log(iceString);
        var ice = JSON.parse(iceString);
        yield peerConnection.addIceCandidate(new RTCIceCandidate(ice));
    });
}
//# sourceMappingURL=Streamer.razor.js.map