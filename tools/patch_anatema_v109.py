from pathlib import Path
import cv2 as cv
import numpy as np
import sys

def nursery(im):
    h,w=im.shape[:2]
    assert (w,h)==(1232,548), (w,h)
    yy,xx=np.mgrid[:h,:w]
    poly=np.array([(741,0),(835,0),(844,34),(855,94),(867,135),(906,193),(910,251),(902,292),(877,318),(853,328),(857,362),(823,369),(813,335),(734,326),(679,324),(661,296),(661,254),(665,192),(687,165),(718,130),(728,91),(734,38)],np.int32)
    mask=np.zeros((h,w),np.uint8);cv.fillPoly(mask,[poly],255)
    def mirror(a,lo,hi):
        n=hi-lo; p=np.mod(a-lo,2*n);return (lo+np.minimum(p,2*n-1-p)).astype(np.float32)
    wall=cv.remap(im,mirror(xx+29,350,397),yy.astype(np.float32),cv.INTER_LINEAR,borderMode=cv.BORDER_REFLECT)
    floor=cv.remap(im,mirror(xx+11,287,391),yy.astype(np.float32),cv.INTER_LINEAR,borderMode=cv.BORDER_REFLECT)
    wall=wall.astype(np.float32);floor=floor.astype(np.float32)
    wall*=.91;floor*=.83
    blend=np.clip((yy-262)/21,0,1)[...,None]
    fill=wall*(1-blend)+floor*blend
    mask=cv.dilate(mask,np.ones((7,7),np.uint8));a=cv.GaussianBlur(mask,(15,15),3).astype(np.float32)[...,None]/255
    return np.clip(im*(1-a)+fill*a,0,255).astype(np.uint8)

def funeral(im):
    h,w=im.shape[:2];assert (w,h)==(1232,548),(w,h)
    yy,xx=np.mgrid[:h,:w];hsv=cv.cvtColor(im,cv.COLOR_BGR2HSV).astype(np.float32)
    hue,sat,val=cv.split(hsv)
    roi=np.clip((57-np.abs(xx-617))/6,0,1)*np.clip((64-np.abs(yy-74))/7,0,1)
    gold=np.clip((hue-9)/7,0,1)*np.clip((40-hue)/8,0,1)*np.clip((sat-55)/45,0,1)*np.clip((val-52)/30,0,1)*roi
    gold=cv.GaussianBlur(gold.astype(np.float32),(3,3),.45)
    grain=(np.random.default_rng(108).random((h,w))-.5)*.15
    shade=.72+.10*np.cos((xx-617)*.058)+.07*np.sin(yy*.13)+grain
    hsv[:,:,0]=hue*(1-gold)+17*gold
    hsv[:,:,1]=sat*(1-.43*gold)
    hsv[:,:,2]=val*(1-gold*(1-shade))
    aged=cv.cvtColor(np.clip(hsv,0,255).astype(np.uint8),cv.COLOR_HSV2BGR)
    a=np.clip(gold*1.8,0,1)[...,None]
    return np.clip(im*(1-a)+aged*a,0,255).astype(np.uint8)

if __name__=='__main__':
    root=Path(sys.argv[1]);out=Path(sys.argv[2]);out.mkdir(parents=True,exist_ok=True)
    for name,fn in [('emile104_map_1.jpg',nursery),('emile107_funeral.jpg',funeral)]:
        im=cv.imread(str(root/name));assert im is not None,name
        assert cv.imwrite(str(out/name),fn(im),[cv.IMWRITE_JPEG_QUALITY,96])
