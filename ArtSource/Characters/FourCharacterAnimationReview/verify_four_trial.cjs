const {chromium}=require('C:/Users/MX_520/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs=require('node:fs'),path=require('node:path'),{pathToFileURL}=require('node:url');
(async()=>{
const base=path.resolve('outputs/FourCharacterTrial'),manifest=JSON.parse(fs.readFileSync(path.join(base,'trial-manifest.json'),'utf8'));
const browser=await chromium.launch({headless:true,executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'});
const page=await browser.newPage({viewport:{width:1440,height:1150}});const errors=[];page.on('pageerror',e=>errors.push(String(e)));const layoutOnly=process.argv.includes('--layout-only');let results=layoutOnly?JSON.parse(fs.readFileSync(path.join(base,'browser-verification.json'),'utf8')).clips:[];
for(const clip of layoutOnly?[]:manifest.clips){
 await page.goto(pathToFileURL(path.join(base,clip.path)).href);await page.waitForFunction(()=>window.animationReview?.ready);
 const data=await page.evaluate(()=>window.animationReview.data);if(data.frames.length!==clip.count)throw Error('frame count mismatch');
 await page.click('#next');if(await page.evaluate(()=>window.animationReview.index)!==1)throw Error('next failed');
 await page.click('#prev');if(await page.evaluate(()=>window.animationReview.index)!==0)throw Error('previous failed');
 await page.check('#onion');await page.check('#silhouette');await page.uncheck('#guides');await page.selectOption('#background','#ddd9d0');
 await page.uncheck('#silhouette');await page.check('#guides');await page.selectOption('#background','#263540');
 await page.selectOption('#speed','2');await page.check('#loop');await page.click('#play');
 const total=data.frames.reduce((n,f)=>n+f.duration,0);await page.waitForTimeout(total+150);
 const advanced=await page.evaluate(()=>window.animationReview.index);if(advanced===0)throw Error('playback not advancing');
 await page.click('#restart');await page.uncheck('#loop');await page.click('#play');await page.waitForTimeout(total/2+180);
 if(await page.evaluate(()=>window.animationReview.index)!==clip.count-1||await page.locator('#play').textContent()!=='播放')throw Error('end hold failed');
 results.push({clip:clip.path,frames:clip.count,loaded:true,stepping:true,controls:true,two_cycles_at_2x:true,end_hold:true});
}
await page.goto(pathToFileURL(path.join(base,'index.html')).href);
await page.waitForFunction(()=>window.fourCharacterTrial);for(const id of ['hero','melee','runner','caster']){await page.click(`[data-role="${id}"]`);if(await page.locator('#clips option').count()!==2)throw Error('role routing failed');const src=await page.locator('#primary').getAttribute('src');if(!src.startsWith(id+'-'))throw Error('wrong role view')}
await page.selectOption('#clips','cast');await page.check('#compare');await page.waitForTimeout(250);
if(await page.locator('#edited').getAttribute('src')!=='caster-cast-edit/review.html')throw Error('compare routing failed');
for(const id of ['primary','edited']){await page.frameLocator('#'+id).locator('#scrub').waitFor();const outer=await page.locator('#'+id).boundingBox(),inner=await page.frameLocator('#'+id).locator('#scrub').boundingBox();if(inner.y+inner.height>outer.y+outer.height)throw Error('Frame scrubber clipped: '+id)}
await page.screenshot({path:path.join(base,'trial-browser-preview.png'),fullPage:true});
if(errors.length)throw Error(errors.join('\n'));
fs.writeFileSync(path.join(base,'browser-verification.json'),JSON.stringify({result:'passed',clips:results,role_switching:true,comparison:true,comparison_scrubbers_visible:true,page_errors:errors},null,2));
console.log(JSON.stringify({clips:results.length,role_switching:true,comparison:true,errors}));await browser.close();
})().catch(e=>{console.error(e);process.exitCode=1});
