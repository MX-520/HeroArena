const {chromium}=require('C:/Users/MX_520/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const path=require('node:path'),fs=require('node:fs'),{pathToFileURL}=require('node:url');
(async()=>{
const base=path.resolve('outputs/GoblinSingleSlash'),browser=await chromium.launch({headless:true,executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'}),page=await browser.newPage({viewport:{width:1440,height:1200}});const errors=[];page.on('pageerror',e=>errors.push(String(e)));
let clips=[];
for(const name of ['revised-review','longblade-review']){
 await page.goto(pathToFileURL(path.join(base,name,'review.html')).href);await page.waitForFunction(()=>window.animationReview?.ready);
 const data=await page.evaluate(()=>window.animationReview.data);if(data.frames.length!==8||data.frames.reduce((s,f)=>s+f.duration,0)!==1000||data.loop!==false)throw Error('bad clip metadata');
 for(let i=0;i<8;i++){await page.locator('#scrub').fill(String(i));if(await page.evaluate(()=>window.animationReview.index)!==i)throw Error('frame sampling failed')}
 await page.check('#onion');await page.check('#silhouette');await page.uncheck('#silhouette');await page.selectOption('#background','#ddd9d0');await page.selectOption('#background','#263540');
 await page.click('#restart');await page.selectOption('#speed','2');await page.click('#play');await page.waitForTimeout(700);
 if(await page.evaluate(()=>window.animationReview.index)!==7||await page.locator('#play').textContent()!=='播放')throw Error('end hold failed');
 clips.push({name,frames:8,duration_ms:1000,all_frames_sampled:true,controls:true,one_shot_end_hold:true});
}
await page.goto(pathToFileURL(path.join(base,'index.html')).href);await page.frameLocator('#new').locator('#scrub').waitFor();
await page.selectOption('#speed','2');await page.check('#loop');await page.click('#playBoth');await page.waitForTimeout(1200);await page.click('#pauseBoth');
await page.selectOption('#frame','5');for(const id of ['old','new'])if(await page.frameLocator('#'+id).locator('#scrub').inputValue()!=='5')throw Error('parent frame control failed');
await page.selectOption('#variant','revised-review/review.html');await page.frameLocator('#new').locator('#scrub').waitFor();await page.selectOption('#frame','4');if(await page.frameLocator('#new').locator('#scrub').inputValue()!=='4')throw Error('short variant failed');
await page.selectOption('#variant','longblade-review/review.html');await page.frameLocator('#new').locator('#scrub').waitFor();await page.selectOption('#frame','0');await page.selectOption('#speed','1');await page.uncheck('#loop');
await page.screenshot({path:path.join(base,'browser-preview.png'),fullPage:true});if(errors.length)throw Error(errors.join('\n'));
fs.writeFileSync(path.join(base,'browser-verification.json'),JSON.stringify({result:'passed',clips,parent_play_pause:true,parent_frame_control:true,sword_variant_switching:true,page_errors:errors},null,2));console.log('PASS: 2 x 8 frames, 1s one-shot, playback/stepping, shared controls and sword-length switch.');await browser.close();
})().catch(e=>{console.error(e);process.exitCode=1});
