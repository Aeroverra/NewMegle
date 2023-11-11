var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
export var localPeerConnection;
export var remotePeerConnection;
export function acceptIce(dotnet, connectionId, iceString, destinationLocal) {
    return __awaiter(this, void 0, void 0, function* () {
        console.log("reccc ice");
        console.log(iceString);
        var ice = JSON.parse(iceString);
        if (destinationLocal == true) {
            yield localPeerConnection.addIceCandidate(new RTCIceCandidate(ice));
        }
        else {
            yield remotePeerConnection.addIceCandidate(new RTCIceCandidate(ice));
        }
    });
}
export function acceptAnswer(dotnet, connectionId, answerString) {
    return __awaiter(this, void 0, void 0, function* () {
        console.log("reccc");
        console.log(answerString);
        var answer = JSON.parse(answerString);
        yield localPeerConnection.setRemoteDescription(new RTCSessionDescription(answer));
    });
}
export function acceptOffer(dotnet, connectionId, offerString) {
    return __awaiter(this, void 0, void 0, function* () {
        remotePeerConnection = new RTCPeerConnection();
        remotePeerConnection.oniceconnectionstatechange = (event) => {
            console.log(`ICE state: ${remotePeerConnection.iceConnectionState}`);
            console.log('ICE state change event: ', event);
        };
        remotePeerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                dotnet.invokeMethodAsync('SendIce', connectionId, JSON.stringify(event.candidate), true);
            }
        };
        remotePeerConnection.ontrack = (event) => {
            const [remoteStream] = event.streams;
            const videoElement = document.querySelector('video#remoteVideo');
            videoElement.srcObject = remoteStream;
            videoElement.autoplay = true;
            videoElement.play().catch(console.error);
        };
        var offer = JSON.parse(offerString);
        yield remotePeerConnection.setRemoteDescription(new RTCSessionDescription(offer));
        const answer = yield remotePeerConnection.createAnswer();
        yield remotePeerConnection.setLocalDescription(answer);
        // Send the answer back to the signaling server to be passed to the share page
        var answerString = JSON.stringify(answer);
        console.log(answerString);
        return answerString;
    });
}
export function createStream() {
    return __awaiter(this, void 0, void 0, function* () {
        var result = yield createPeerConnection();
        return result;
    });
}
export function getLocalStream() {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const stream = yield navigator.mediaDevices.getUserMedia({ video: true, audio: true });
            const videoElement = document.querySelector('video#localVideo');
            videoElement.srcObject = stream;
            videoElement.autoplay = true;
            videoElement.play().catch(console.error);
            return stream;
        }
        catch (error) {
            console.error('Error accessing media devices.', error);
        }
    });
}
export function createPeerConnection() {
    return __awaiter(this, void 0, void 0, function* () {
        const localStream = yield getLocalStream();
        localPeerConnection = new RTCPeerConnection();
        localStream.getTracks().forEach(track => {
            localPeerConnection.addTrack(track, localStream);
        });
        const offer = yield localPeerConnection.createOffer();
        yield localPeerConnection.setLocalDescription(offer);
        var offerString = JSON.stringify(offer);
        console.log(offerString);
        return JSON.stringify(offer);
    });
}
//# sourceMappingURL=Streamer.razor.js.map