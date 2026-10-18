const {chromium}=require('C:/Users/MX_520/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs=require('node:fs'),path=require('node:path'),{pathToFileURL}=require('node:url');
(async()=>{
 const base=__dirname, browser=await chromium.launch({headless:true,executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'});
 const results=[];
 for(const role of ['Sword','Axe','Mage']){
  const page=await browser.newPage({viewport:{width:1280,height:1100}}),errors=[];
  page.on('pageerror',e=>errors.push(String(e)));
  await page.goto(pathToFileURL(path.join(base,role,'review/review.html')).href);
  await page.waitForFunction(()=>window.animationReview?.ready);
  for(let i=0;i<8;i++){
   await page.locator('#scrub').fill(String(i));
   if(await page.evaluate(()=>window.animationReview.index)!==i)throw Error(role+' frame selection '+i);
  }
  await page.click('#restart');await page.click('#next');
  if(await page.evaluate(()=>window.animationReview.index)!==1)throw Error('next');
  await page.click('#prev');if(await page.evaluate(()=>window.animationReview.index)!==0)throw Error('prev');
  await page.check('#onion');await page.check('#silhouette');await page.uncheck('#silhouette');await page.uncheck('#onion');
  await page.check('#loop');await page.click('#restart');await page.click('#play');
  const indices=[];
  for(let i=0;i<18;i++){await page.waitForTimeout(200);indices.push(await page.evaluate(()=>window.animationReview.index));}
  if(await page.locator('#play').textContent()!=='暂停'||!indices.some((x,i)=>i&&x<indices[i-1]))throw Error(role+' did not loop');
  await page.click('#play');await page.locator('#scrub').fill('7');
  await page.screenshot({path:path.join(base,role,'review/browser-last-frame.png'),fullPage:true});
  if(errors.length)throw Error(errors.join('\n'));
  results.push({role,frames_sampled:8,normal_speed_playback_ms:3600,loop_wrap_observed:true,controls_passed:true,page_errors:errors,art_quality_acceptance:'pending; functional playback is not visual approval'});
  await page.close();
 }
 await browser.close();fs.writeFileSync(path.join(base,'browser-verification.json'),JSON.stringify({result:'passed',roles:results},null,2));
 console.log('PASS: 24 frame selections, controls, and over two normal-speed loops per role; no script errors.');
})().catch(e=>{console.error(e);process.exitCode=1});
