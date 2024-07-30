Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AA0091/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "收費基本檔清單查詢";

    var T1Rec = 0;
    var T1LastRec = null;
    var T1GetExcel = '../../../api/AA0091/Excel';

    var GetOrderDCFlag = '../../../api/AA0091/GetOrderDCFlag';
    var GetCaseFrom = '../../../api/AA0091/GetCaseFrom';
    var GetInsuSignI = '../../../api/AA0091/GetInsuSignI';
    var GetInsuSignO = '../../../api/AA0091/GetInsuSignO';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var OrderDCFlagStore = Ext.create('Ext.data.Store', {  //停用狀況的store
        fields: ['VALUE', 'TEXT']
    });

    var CaseFromStore = Ext.create('Ext.data.Store', {  //標案來源的store
        fields: ['VALUE', 'TEXT']
    });

    var InsuSignIStore = Ext.create('Ext.data.Store', {  //健保給付狀況(住院)的store
        fields: ['VALUE', 'TEXT']
    });

    var InsuSignOStore = Ext.create('Ext.data.Store', {  //健保給付狀況(門診)的store
        fields: ['VALUE', 'TEXT']
    });

    function SetOrderDCFlag() { //建立停用狀況的下拉式選單
        Ext.Ajax.request({
            url: GetOrderDCFlag,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var orderdcflag = data.etts;
                    if (orderdcflag.length > 0) {
                        OrderDCFlagStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < orderdcflag.length; i++) {
                            OrderDCFlagStore.add({ VALUE: orderdcflag[i].VALUE, TEXT: orderdcflag[i].TEXT });
                        }
                    }
                    T1QueryForm.getForm().findField("P12").setValue("");
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function SetCaseFrom() { //建立標案來源的下拉式選單
        Ext.Ajax.request({
            url: GetCaseFrom,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var casefrom = data.etts;
                    if (casefrom.length > 0) {
                        CaseFromStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < casefrom.length; i++) {
                            CaseFromStore.add({ VALUE: casefrom[i].VALUE, TEXT: casefrom[i].TEXT });
                        }
                    }
                    T1QueryForm.getForm().findField("P15").setValue("");
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function SetInsuSignI() { //建立健保給付狀況(住院)的下拉式選單
        Ext.Ajax.request({
            url: GetInsuSignI,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var insusigni = data.etts;
                    if (insusigni.length > 0) {
                        InsuSignIStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < insusigni.length; i++) {
                            InsuSignIStore.add({ VALUE: insusigni[i].VALUE, TEXT: insusigni[i].TEXT });
                        }
                    }
                    T1QueryForm.getForm().findField("P16").setValue("");
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function SetInsuSignO() { //建立健保給付狀況(門診)的下拉式選單
        Ext.Ajax.request({
            url: GetInsuSignO,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var insusigno = data.etts;
                    if (insusigno.length > 0) {
                        InsuSignOStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < insusigno.length; i++) {
                            InsuSignOStore.add({ VALUE: insusigno[i].VALUE, TEXT: insusigno[i].TEXT });
                        }
                    }
                    T1QueryForm.getForm().findField("P17").setValue("");
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var getTodayDate = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        var d = (new Date().getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + m + d;
    }

    // 查詢欄位
    var T1QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        //frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        //bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 120,
            width: 250
        },
        items: [{
            xtype: 'container',
            defaultType: 'textfield',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '院內碼範圍',
                    name: 'P00',
                    id: 'P00',
                    enforceMaxLength: true,
                    maxLength: 10
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '至',
                    name: 'P01',
                    id: 'P01',
                    enforceMaxLength: true,
                    maxLength: 10,
                    labelSeparator: ''
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '廠商代碼範圍',
                    name: 'P02',
                    id: 'P02',
                    enforceMaxLength: true,
                    maxLength: 8
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '至',
                    name: 'P03',
                    id: 'P03',
                    enforceMaxLength: true,
                    maxLength: 8,
                    labelSeparator: ''
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '健保碼範圍',
                    name: 'P04',
                    id: 'P04',
                    enforceMaxLength: true,
                    maxLength: 16
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '至',
                    name: 'P05',
                    id: 'P05',
                    enforceMaxLength: true,
                    maxLength: 16,
                    labelSeparator: ''
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '廠牌範圍',
                    name: 'P06',
                    id: 'P06',
                    enforceMaxLength: true,
                    maxLength: 8,
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '至',
                    name: 'P07',
                    id: 'P07',
                    enforceMaxLength: true,
                    maxLength: 8,
                    labelSeparator: ''
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '建檔日期',
                    name: 'P08',
                    editable: false,
                    maxLength: 13
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P09',
                    editable: false,
                    maxLength: 13,
                    labelSeparator: ''
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '異動日期',
                    name: 'P10',
                    editable: false,
                    maxLength: 13,
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P11',
                    editable: false,
                    maxLength: 13,
                    labelSeparator: ''
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'combo',
                    fieldLabel: '停用狀況',
                    name: 'P12',
                    id: 'P12',
                    store: OrderDCFlagStore,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    autoSelect: true,
                    anyMatch: true,
                    editable: false,
                    //maxLength: 1,
                    listeners: {
                        change: function () {
                        }
                    }
                },
                {
                    xtype: 'combo',
                    fieldLabel: '查詢類別',
                    name: 'P13',
                    id: 'P13',
                    //store: CLSNAMEStore,
                    //queryMode: 'local',
                    store: [
                        { VALUE: '', TEXT: '' },
                        { VALUE: '0', TEXT: '0 非藥品非衛材' },
                        { VALUE: '1', TEXT: '1 藥品' },
                    ],
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    autoSelect: true,
                    anyMatch: true,
                    editable: false,
                    //maxLength: 1,
                    listeners: {
                        change: function () {
                        }
                    }
                },
                {
                    xtype: 'combo',
                    fieldLabel: '有無健保碼',
                    name: 'P14',
                    id: 'P14',
                    store: [
                        { VALUE: '', TEXT: '' },
                        { VALUE: 'Y', TEXT: 'Y 有' },
                        { VALUE: 'N', TEXT: 'N 無' },
                    ],
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    autoSelect: true,
                    anyMatch: true,
                    editable: false,
                    //maxLength: 1,
                    listeners: {
                        change: function () {
                        }
                    }
                },
                {
                    xtype: 'combo',
                    fieldLabel: '標案來源',
                    name: 'P15',
                    id: 'P15',
                    store: CaseFromStore,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    autoSelect: true,
                    anyMatch: true,
                    editable: false,
                    //maxLength: 1,
                    listeners: {
                        change: function () {
                        }
                    }
                },
                {
                    xtype: 'combo',
                    fieldLabel: '健保給付狀況(住院)',
                    name: 'P16',
                    id: 'P16',
                    store: InsuSignIStore,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    autoSelect: true,
                    anyMatch: true,
                    editable: false,
                    //maxLength: 1,
                    listeners: {
                        change: function () {
                        }
                    }
                },
                {
                    xtype: 'combo',
                    fieldLabel: '健保給付狀況(門診)',
                    name: 'P17',
                    id: 'P17',
                    store: InsuSignOStore,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    autoSelect: true,
                    anyMatch: true,
                    editable: false,
                    //maxLength: 1,
                    listeners: {
                        change: function () {
                        }
                    }
                }
            ]
        }],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                msglabel("");
                T1Load();
                T1Grid.down('#export').setDisabled(false);
            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                T1QueryForm.getForm().findField('P00').setValue("");
                T1QueryForm.getForm().findField('P01').setValue("");
                T1QueryForm.getForm().findField('P02').setValue("");
                T1QueryForm.getForm().findField('P03').setValue("");
                T1QueryForm.getForm().findField('P04').setValue("");
                T1QueryForm.getForm().findField('P05').setValue("");
                T1QueryForm.getForm().findField('P06').setValue("");
                T1QueryForm.getForm().findField('P07').setValue("");
                T1QueryForm.getForm().findField('P08').setValue("");
                T1QueryForm.getForm().findField('P09').setValue("");
                T1QueryForm.getForm().findField('P10').setValue("");
                T1QueryForm.getForm().findField('P11').setValue("");
                T1QueryForm.getForm().findField('P12').setValue("");
                T1QueryForm.getForm().findField('P13').setValue("");
                T1QueryForm.getForm().findField('P14').setValue("");
                T1QueryForm.getForm().findField('P15').setValue("");
                T1QueryForm.getForm().findField('P16').setValue("");
                T1QueryForm.getForm().findField('P17').setValue("");
                f.findField('P00').focus(); // 進入畫面時輸入游標預設在P0
            }
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0091VM', { // 定義於/Scripts/app/store/AA/AA0091VM.js
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p00: T1QueryForm.getForm().findField('P00').getValue(),
                    p01: T1QueryForm.getForm().findField('P01').getValue(),
                    p02: T1QueryForm.getForm().findField('P02').getValue(),
                    p03: T1QueryForm.getForm().findField('P03').getValue(),
                    p04: T1QueryForm.getForm().findField('P04').getValue(),
                    p05: T1QueryForm.getForm().findField('P05').getValue(),
                    p06: T1QueryForm.getForm().findField('P06').getValue(),
                    p07: T1QueryForm.getForm().findField('P07').getValue(),
                    p08: T1QueryForm.getForm().findField('P08').getValue(),
                    p09: T1QueryForm.getForm().findField('P09').getValue(),
                    p10: T1QueryForm.getForm().findField('P10').getValue(),
                    p11: T1QueryForm.getForm().findField('P11').getValue(),
                    p12: T1QueryForm.getForm().findField('P12').getValue(),
                    p13: T1QueryForm.getForm().findField('P13').getValue(),
                    p14: T1QueryForm.getForm().findField('P14').getValue(),
                    p15: T1QueryForm.getForm().findField('P15').getValue(),
                    p16: T1QueryForm.getForm().findField('P16').getValue(),
                    p17: T1QueryForm.getForm().findField('P17').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export',
                text: '匯出',
                handler: function () {
                    var today = getTodayDate();
                    var p = new Array();
                    p.push({ name: 'FN', value: today + '_收費基本檔清單.xls' });
                    p.push({ name: 'p00', value: T1QueryForm.getForm().findField('P00').getValue() });
                    p.push({ name: 'p01', value: T1QueryForm.getForm().findField('P01').getValue() });
                    p.push({ name: 'p02', value: T1QueryForm.getForm().findField('P02').getValue() });
                    p.push({ name: 'p03', value: T1QueryForm.getForm().findField('P03').getValue() });
                    p.push({ name: 'p04', value: T1QueryForm.getForm().findField('P04').getValue() });
                    p.push({ name: 'p05', value: T1QueryForm.getForm().findField('P05').getValue() });
                    p.push({ name: 'p06', value: T1QueryForm.getForm().findField('P06').getValue() });
                    p.push({ name: 'p07', value: T1QueryForm.getForm().findField('P07').getValue() });
                    p.push({ name: 'p08', value: T1QueryForm.getForm().findField('P08').rawValue });
                    p.push({ name: 'p09', value: T1QueryForm.getForm().findField('P09').rawValue });
                    p.push({ name: 'p10', value: T1QueryForm.getForm().findField('P10').rawValue });
                    p.push({ name: 'p11', value: T1QueryForm.getForm().findField('P11').rawValue });
                    p.push({ name: 'p12', value: T1QueryForm.getForm().findField('P12').getValue() });
                    p.push({ name: 'p13', value: T1QueryForm.getForm().findField('P13').getValue() });
                    p.push({ name: 'p14', value: T1QueryForm.getForm().findField('P14').getValue() });
                    p.push({ name: 'p15', value: T1QueryForm.getForm().findField('P15').getValue() });
                    p.push({ name: 'p16', value: T1QueryForm.getForm().findField('P16').getValue() });
                    p.push({ name: 'p17', value: T1QueryForm.getForm().findField('P17').getValue() });
                    PostForm(T1GetExcel, p);
                }
            }
        ]
    });


    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        //bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'displayfield',
        items: [
            //    {
            //    fieldLabel: 'Update',
            //    name: 'x',
            //    xtype: 'hidden'
            //},
            {
                fieldLabel: '院內代碼',
                name: 'ORDERCODE',
                readOnly: true
            },
            {
                fieldLabel: '英文名稱',
                name: 'ORDERENGNAME',
                readOnly: true
            },
            {
                fieldLabel: '中文名稱',
                name: 'ORDERCHINNAME',
                readOnly: true
            },
            {
                fieldLabel: '成份名稱',
                name: 'SCIENTIFICNAME',
                readOnly: true
            },
            {
                fieldLabel: '醫囑單位',
                name: 'ORDERUNIT',
                readOnly: true
            },
            {
                fieldLabel: '中文單位',
                name: 'ORDERCHINUNIT',
                readOnly: true
            },
            {
                fieldLabel: '劑型單位',
                name: 'ATTACHUNIT',
                readOnly: true
            },
            {
                fieldLabel: '扣庫單位',
                name: 'STOCKUNIT',
                readOnly: true
            },
            {
                fieldLabel: '藥品藥材代碼',
                name: 'SKORDERCODE',
                readOnly: true
            },
            {
                fieldLabel: 'UD服務',
                name: 'UDSERVICEFLAG',
                readOnly: true
            },
            {
                fieldLabel: '服用藥別',
                name: 'TAKEKIND',
                readOnly: true
            },
            {
                fieldLabel: '門診倍數核發',
                name: 'LIMITEDQTYO',
                readOnly: true
            },
            {
                fieldLabel: '住院倍數核發',
                name: 'LIMITEDQTYI',
                readOnly: true
            },
            {
                fieldLabel: '買斷藥',
                name: 'BUYORDERFLAG',
                readOnly: true
            },
            {
                fieldLabel: '系統啟用日期',
                name: 'OPENDATE',
                readOnly: true
            },
            {
                fieldLabel: '公藥否',
                name: 'PUBLICDRUGFLAG',
                readOnly: true
            },
            {
                fieldLabel: '開始日期',
                name: 'STARTDATE',
                readOnly: true
            },
            {
                fieldLabel: '院內名稱',
                name: 'ORDERHOSPNAME',
                readOnly: true
            },
            {
                fieldLabel: '簡稱',
                name: 'ORDEREASYNAME',
                readOnly: true
            },
            {
                fieldLabel: '保險給付否',
                name: 'INSUOFFERFLAG',
                readOnly: true
            },
            {
                fieldLabel: 'DCL',
                name: 'DCL',
                readOnly: true
            },
            {
                fieldLabel: '健保帶材料否',
                name: 'APPENDMATERIALFLAG',
                readOnly: true
            },
            {
                fieldLabel: '特殊品項',
                name: 'EXORDERFLAG',
                readOnly: true
            },
            {
                fieldLabel: '停用碼',
                name: 'ORDERDCFLAG',
                readOnly: true
            },
            {
                fieldLabel: '一次極量',
                name: 'MAXQTYPERTIME',
                readOnly: true
            },
            {
                fieldLabel: '一日極量',
                name: 'MAXQTYPERDAY',
                readOnly: true
            },
            {
                fieldLabel: '限制次數',
                name: 'MAXTAKETIMES',
                readOnly: true
            },
            {
                fieldLabel: '衛生署核准字號',
                name: 'DOHLICENSENO',
                readOnly: true
            },
            {
                fieldLabel: 'RFID條碼',
                name: 'RFIDCODE',
                readOnly: true
            },
            {
                fieldLabel: '給藥途徑(部位)代碼',
                name: 'PATHNO',
                readOnly: true
            },
            {
                fieldLabel: '累計用藥',
                name: 'AGGREGATECODE',
                readOnly: true
            },
            {
                fieldLabel: '開立專用限制',
                name: 'LIMITFLAG',
                readOnly: true
            },
            {
                fieldLabel: '管制用藥',
                name: 'RESTRICTCODE',
                readOnly: true
            },
            {
                fieldLabel: '抗生素等級',
                name: 'ANTIBIOTICSCODE',
                readOnly: true
            },
            {
                fieldLabel: '住消耗歸整',
                name: 'CARRYKINDI',
                readOnly: true
            },
            {
                fieldLabel: '門急消耗歸整',
                name: 'CARRYKINDO',
                readOnly: true
            },
            {
                fieldLabel: 'UD磨粉',
                name: 'UDPOWDERFLAG',
                readOnly: true
            },
            {
                fieldLabel: '合理回流藥',
                name: 'RETURNDRUGFLAG',
                readOnly: true
            },
            {
                fieldLabel: '研究用藥',
                name: 'RESEARCHDRUGFLAG',
                readOnly: true
            },
            {
                fieldLabel: '藥包機品項',
                name: 'MACHINEFLAG',
                readOnly: true
            },
            {
                fieldLabel: '結轉計價',
                name: 'TRANSCOMPUTEFLAG',
                readOnly: true
            },
            {
                fieldLabel: '限制途徑',
                name: 'FIXPATHNOFLAG',
                readOnly: true
            },
            {
                fieldLabel: '適應症(中文)',
                name: 'SYMPTOMCHIN',
                readOnly: true
            },
            {
                fieldLabel: '適應症(英文)',
                name: 'SYMPTOMENG',
                readOnly: true
            },
            {
                fieldLabel: '不可剝半',
                name: 'ONLYROUNDFLAG',
                readOnly: true
            },
            {
                fieldLabel: '不可磨粉',
                name: 'UNABLEPOWDERFLAG',
                readOnly: true
            },
            {
                fieldLabel: '高警訊藥品',
                name: 'DANGERDRUGFLAG',
                readOnly: true
            },
            {
                fieldLabel: '高警訊藥品提示',
                name: 'DANGERDRUGMEMO',
                readOnly: true
            },
            {
                fieldLabel: '冷藏存放',
                name: 'COLDSTORAGEFLAG',
                readOnly: true
            },
            {
                fieldLabel: '避光存放',
                name: 'LIGHTAVOIDFLAG',
                readOnly: true
            },
            {
                fieldLabel: '異動狀態',
                name: 'CHANGESTATUS',
                readOnly: true
            },
            {
                fieldLabel: '門診給藥頻率',
                name: 'FREQNOO',
                readOnly: true
            },
            {
                fieldLabel: '住院給藥頻率',
                name: 'FREQNOI',
                readOnly: true
            },
            {
                fieldLabel: '預設開立天數',
                name: 'ORDERDAYS',
                readOnly: true
            },
            {
                fieldLabel: '劑量',
                name: 'DOSE',
                readOnly: true
            },
            {
                fieldLabel: '院內費用類別',
                name: 'HOSPCHARGEID1',
                readOnly: true
            },
            {
                fieldLabel: '院內費用',
                name: 'HOSPCHARGEID2',
                readOnly: true
            },
            {
                fieldLabel: '健保費用類別',
                name: 'INSUCHARGEID1',
                readOnly: true
            },
            {
                fieldLabel: '健保費用',
                name: 'INSUCHARGEID2',
                readOnly: true
            },
            {
                fieldLabel: '醫令類別',
                name: 'ORDERTYPE',
                readOnly: true
            },
            {
                fieldLabel: '醫令類別(申報定義)',
                name: 'ORDERKIND',
                readOnly: true
            },
            {
                fieldLabel: '高價用藥',
                name: 'HIGHPRICEFLAG',
                readOnly: true
            },
            {
                fieldLabel: '特殊治療種類',
                name: 'CURETYPE',
                readOnly: true
            },
            {
                fieldLabel: '住院醫囑顯示',
                name: 'INPDISPLAYFLAG',
                readOnly: true
            },
            {
                fieldLabel: '開立醫令即為報到',
                name: 'SOONCULLFLAG',
                readOnly: true
            },
            {
                fieldLabel: '替代院內代碼1',
                name: 'SUBSTITUTE1',
                readOnly: true
            },
            {
                fieldLabel: '替代院內代碼2',
                name: 'SUBSTITUTE2',
                readOnly: true
            },
            {
                fieldLabel: '替代院內代碼3',
                name: 'SUBSTITUTE3',
                readOnly: true
            },
            {
                fieldLabel: '替代院內代碼4',
                name: 'SUBSTITUTE4',
                readOnly: true
            },
            {
                fieldLabel: '替代院內代碼5',
                name: 'SUBSTITUTE5',
                readOnly: true
            },
            {
                fieldLabel: '連帶否(門診)',
                name: 'RELATEFLAGO',
                readOnly: true
            },
            {
                fieldLabel: '連帶否(住院)',
                name: 'RELATEFLAGI',
                readOnly: true
            },
            {
                fieldLabel: '體重及安全量：計算別',
                name: 'WEIGHTTYPE',
                readOnly: true
            },
            {
                fieldLabel: '體重及安全量：限制數量',
                name: 'WEIGHTUNITLIMIT',
                readOnly: true
            },
            {
                fieldLabel: '限制狀態',
                name: 'RESTRICTTYPE',
                readOnly: true
            },
            {
                fieldLabel: '門診限制開立數量',
                name: 'MAXQTYO',
                readOnly: true
            },
            {
                fieldLabel: '住院限制開立數量',
                name: 'MAXQTYI',
                readOnly: true
            },
            {
                fieldLabel: '門診限制開立日數',
                name: 'MAXDAYSO',
                readOnly: true
            },
            {
                fieldLabel: '住院限制開立日數',
                name: 'MAXDAYSI',
                readOnly: true
            },
            {
                fieldLabel: '門診效期日數',
                name: 'VALIDDAYSO',
                readOnly: true
            },
            {
                fieldLabel: '住院效期日數',
                name: 'VALIDDAYSI',
                readOnly: true
            },
            {
                fieldLabel: '是否需報到',
                name: 'CHECKINSWITCH',
                readOnly: true
            },
            {
                fieldLabel: '是否發報告',
                name: 'REPORTFLAG',
                readOnly: true
            },
            {
                fieldLabel: 'Single Item',
                name: 'SINGLEITEMFLAG',
                readOnly: true
            },
            {
                fieldLabel: '手術碼',
                name: 'OPERATIONFLAG',
                readOnly: true
            },
            {
                fieldLabel: '檢驗用藥',
                name: 'EXAMINEDRUGFLAG',
                readOnly: true
            },
            {
                fieldLabel: '特定治療項目',
                name: 'MAINCUREITEM',
                readOnly: true
            },
            {
                fieldLabel: '放射部位',
                name: 'RAYPOSITION',
                readOnly: true
            },
            {
                fieldLabel: '空針代碼',
                name: 'CCORDERCODE',
                readOnly: true
            },
            {
                fieldLabel: '放射第二張代碼',
                name: 'XRAYORDERCODE',
                readOnly: true
            },
            {
                fieldLabel: '手術診斷碼',
                name: 'ORDIAGCODE',
                readOnly: true
            },
            {
                fieldLabel: '醫令排序',
                name: 'ORDERCODESORT',
                readOnly: true
            },
            {
                fieldLabel: '傳送單位否',
                name: 'SENDUNITFLAG',
                readOnly: true
            },
            {
                fieldLabel: '是否需 Sign In',
                name: 'SIGNFLAG',
                readOnly: true
            },
            {
                fieldLabel: '除外項目',
                name: 'EXCLUDEFLAG',
                readOnly: true
            },
            {
                fieldLabel: '處置需報調劑方式',
                name: 'NEEDOPDTYPEFLAG',
                readOnly: true
            },
            {
                fieldLabel: '藥品成份Element1',
                name: 'DRUGELEMCODE1',
                readOnly: true
            },
            {
                fieldLabel: '藥品成份Element2',
                name: 'DRUGELEMCODE2',
                readOnly: true
            },
            {
                fieldLabel: '藥品成份Element3',
                name: 'DRUGELEMCODE3',
                readOnly: true
            },
            {
                fieldLabel: '藥品成份Element4',
                name: 'DRUGELEMCODE4',
                readOnly: true
            },
            {
                fieldLabel: '可線上異動否',
                name: 'CHANGEABLEFLAG',
                readOnly: true
            },
            {
                fieldLabel: 'TDM 藥品',
                name: 'TDMFLAG',
                readOnly: true
            },
            {
                fieldLabel: '專案碼',
                name: 'SPECIALORDERKIND',
                readOnly: true
            },
            {
                fieldLabel: '處置需報部位',
                name: 'NEEDREGIONFLAG',
                readOnly: true
            },
            {
                fieldLabel: '醫令使用狀態',
                name: 'ORDERUSETYPE',
                readOnly: true
            },
            {
                fieldLabel: '預設劑量',
                name: 'FIXDOSEFLAG',
                readOnly: true
            },
            {
                fieldLabel: '罕見疾病用藥',
                name: 'RAREDISORDERFLAG',
                readOnly: true
            },
            {
                fieldLabel: '內審用藥',
                name: 'HOSPEXAMINEFLAG',
                readOnly: true
            },
            {
                fieldLabel: '給付條文代碼',
                name: 'ORDERCONDCODE',
                readOnly: true
            },
            {
                fieldLabel: '內審限制用量',
                name: 'HOSPEXAMINEQTYFLAG',
                readOnly: true
            },
            {
                fieldLabel: '記錄建立日期/時間',
                name: 'CREATEDATETIME',
                readOnly: true
            },
            {
                fieldLabel: '記錄建立人員',
                name: 'CREATEOPID',
                readOnly: true
            },
            {
                fieldLabel: '記錄處理日期/時間',
                name: 'PROCDATETIME',
                readOnly: true
            },
            {
                fieldLabel: '記錄處理人員',
                name: 'PROCOPID',
                readOnly: true
            },
            {
                fieldLabel: 'BC肝用藥註記',
                name: 'HEPATITISCODE',
                readOnly: true
            },
            //{
            //    fieldLabel: '院內代碼',
            //    name: 'OrderCode',
            //    readOnly: true
            //},
            {
                fieldLabel: '生效起日',
                name: 'BEGINDATE',
                readOnly: true
            },
            {
                fieldLabel: '生效迄日',
                name: 'ENDDATE',
                readOnly: true
            },
            {
                fieldLabel: '門診扣庫轉換量',
                name: 'STOCKTRANSQTYO',
                readOnly: true
            },
            {
                fieldLabel: '住院扣庫轉換量',
                name: 'STOCKTRANSQTYI',
                readOnly: true
            },
            {
                fieldLabel: '門診劑型數量',
                name: 'ATTACHTRANSQTYO',
                readOnly: true
            },
            {
                fieldLabel: '住院劑型數量',
                name: 'ATTACHTRANSQTYI',
                readOnly: true
            },
            {
                fieldLabel: '健保點數一',
                name: 'INSUAMOUNT1',
                readOnly: true
            },
            {
                fieldLabel: '健保點數二',
                name: 'INSUAMOUNT2',
                readOnly: true
            },
            {
                fieldLabel: '自費點數一',
                name: 'PAYAMOUNT1',
                readOnly: true
            },
            {
                fieldLabel: '自費點數二',
                name: 'PAYAMOUNT2',
                readOnly: true
            },
            {
                fieldLabel: '進價',
                name: 'COSTAMOUNT',
                readOnly: true
            },
            {
                fieldLabel: '是否收管理費',
                name: 'MAMAGEFLAG',
                readOnly: true
            },
            {
                fieldLabel: '管理費%',
                name: 'MAMAGERATE',
                readOnly: true
            },
            {
                fieldLabel: '健保碼',
                name: 'INSUORDERCODE',
                readOnly: true
            },
            {
                fieldLabel: '健保負擔碼(住院)',
                name: 'INSUSIGNI',
                readOnly: true
            },
            {
                fieldLabel: '健保負擔碼(門診)',
                name: 'INSUSIGNO',
                readOnly: true
            },
            {
                fieldLabel: '健保急件加成',
                name: 'INSUEMGFLAG',
                readOnly: true
            },
            {
                fieldLabel: '自費急件加成',
                name: 'HOSPEMGFLAG',
                readOnly: true
            },
            {
                fieldLabel: '牙科轉診加成',
                name: 'DENTALREFFLAG',
                readOnly: true
            },
            {
                fieldLabel: '提成類別碼',
                name: 'PPFTYPE',
                readOnly: true
            },
            {
                fieldLabel: '提成百分比',
                name: 'PPFPERCENTAGE',
                readOnly: true
            },
            {
                fieldLabel: '兒童加成否',
                name: 'INSUKIDFLAG',
                readOnly: true
            },
            {
                fieldLabel: '自費兒童加成',
                name: 'HOSPKIDFLAG',
                readOnly: true
            },
            {
                fieldLabel: '合約單價',
                name: 'CONTRACTPRICE',
                readOnly: true
            },
            {
                fieldLabel: '合約碼',
                name: 'CONTRACNO',
                readOnly: true
            },
            {
                fieldLabel: '廠商代碼(供應商代碼)',
                name: 'SUPPLYNO',
                readOnly: true
            },
            {
                fieldLabel: '標案來源',
                name: 'CASEFROM',
                readOnly: true
            },
            {
                fieldLabel: '檢查折扣否',
                name: 'EXAMINEDISCFLAG',
                readOnly: true
            },
            {
                fieldLabel: '製造廠名稱',
                name: 'ORIGINALPRODUCER',
                readOnly: true
            },
            {
                fieldLabel: '申請商名稱',
                name: 'AGENTNAME',
                readOnly: true
            },
            //{
            //    fieldLabel: '記錄建立日期/時間',
            //    name: 'CreateDateTime',
            //    readOnly: true
            //},
            //{
            //    fieldLabel: '記錄建立人員',
            //    name: 'CreateOpID',
            //    readOnly: true
            //},
            //{
            //    fieldLabel: '記錄處理日期/時間',
            //    name: 'ProcDateTime',
            //    readOnly: true
            //},
            //{
            //    fieldLabel: '記錄處理人員',
            //    name: 'ProcOpID',
            //    readOnly: true
            //},
            {
                fieldLabel: '結束日期時間',
                name: 'ENDDATETIME',
                readOnly: true
            }]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer',
            width: 50
        },
        {
            text: "院內代碼",
            dataIndex: 'ORDERCODE',
            width: 100
        },
        {
            text: "英文名稱",
            dataIndex: 'ORDERENGNAME',
            width: 350
        },
        {
            text: "中文名稱",
            dataIndex: 'ORDERCHINNAME',
            width: 300
        },
        {
            text: "成份名稱",
            dataIndex: 'SCIENTIFICNAME',
            width: 200
        },
        {
            text: "醫囑單位",
            dataIndex: 'ORDERUNIT',
            width: 100
        },
        {
            text: "中文單位",
            dataIndex: 'ORDERCHINUNIT',
            width: 100
        },
        {
            text: "劑型單位",
            dataIndex: 'ATTACHUNIT',
            width: 100
        },
        {
            text: "扣庫單位",
            dataIndex: 'STOCKUNIT',
            width: 100
        },
        {
            text: "藥品藥材代碼",
            dataIndex: 'SKORDERCODE',
            width: 100
        },
        {
            text: "UD服務",
            dataIndex: 'UDSERVICEFLAG',
            width: 100
        },
        {
            text: "服用藥別",
            dataIndex: 'TAKEKIND',
            width: 100
        },
        {
            text: "門診倍數核發",
            dataIndex: 'LIMITEDQTYO',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "住院倍數核發",
            dataIndex: 'LIMITEDQTYI',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "買斷藥",
            dataIndex: 'BUYORDERFLAG',
            width: 100
        },
        {
            text: "系統啟用日期",
            dataIndex: 'OPENDATE',
            width: 100
        },
        {
            text: "公藥否",
            dataIndex: 'PUBLICDRUGFLAG',
            width: 100
        },
        {
            text: "開始日期",
            dataIndex: 'STARTDATE',
            width: 100
        },
        {
            text: "院內名稱",
            dataIndex: 'ORDERHOSPNAME',
            width: 300
        },
        {
            text: "簡稱",
            dataIndex: 'ORDEREASYNAME',
            width: 300
        },
        {
            text: "保險給付否",
            dataIndex: 'INSUOFFERFLAG',
            width: 100
        },
        {
            text: "DCL",
            dataIndex: 'DCL',
            width: 100
        },
        {
            text: "健保帶材料否",
            dataIndex: 'APPENDMATERIALFLAG',
            width: 100
        },
        {
            text: "特殊品項",
            dataIndex: 'EXORDERFLAG',
            width: 100
        },
        {
            text: "停用碼",
            dataIndex: 'ORDERDCFLAG',
            width: 100
        },
        {
            text: "一次極量",
            dataIndex: 'MAXQTYPERTIME',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "一日極量",
            dataIndex: 'MAXQTYPERDAY',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "限制次數",
            dataIndex: 'MAXTAKETIMES',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "衛生署核准字號",
            dataIndex: 'DOHLICENSENO',
            width: 150
        },
        {
            text: "RFID條碼",
            dataIndex: 'RFIDCODE',
            width: 100
        },
        {
            text: "給藥途徑(部位)代碼",
            dataIndex: 'PATHNO',
            width: 120
        },
        {
            text: "累計用藥",
            dataIndex: 'AGGREGATECODE',
            width: 100
        },
        {
            text: "開立專用限制",
            dataIndex: 'LIMITFLAG',
            width: 100
        },
        {
            text: "管制用藥",
            dataIndex: 'RESTRICTCODE',
            width: 100
        },
        {
            text: "抗生素等級",
            dataIndex: 'ANTIBIOTICSCODE',
            width: 100
        },
        {
            text: "住消耗歸整",
            dataIndex: 'CARRYKINDI',
            width: 100
        },
        {
            text: "門急消耗歸整",
            dataIndex: 'CARRYKINDO',
            width: 100
        },
        {
            text: "UD磨粉",
            dataIndex: 'UDPOWDERFLAG',
            width: 100
        },
        {
            text: "合理回流藥",
            dataIndex: 'RETURNDRUGFLAG',
            width: 100
        },
        {
            text: "研究用藥",
            dataIndex: 'RESEARCHDRUGFLAG',
            width: 100
        },
        {
            text: "藥包機品項",
            dataIndex: 'MACHINEFLAG',
            width: 100
        },
        {
            text: "結轉計價",
            dataIndex: 'TRANSCOMPUTEFLAG',
            width: 100
        },
        {
            text: "限制途徑",
            dataIndex: 'FIXPATHNOFLAG',
            width: 100
        },
        {
            text: "適應症(中文)",
            dataIndex: 'SYMPTOMCHIN',
            width: 100
        },
        {
            text: "適應症(英文)",
            dataIndex: 'SYMPTOMENG',
            width: 100
        },
        {
            text: "不可剝半",
            dataIndex: 'ONLYROUNDFLAG',
            width: 100
        },
        {
            text: "不可磨粉",
            dataIndex: 'UNABLEPOWDERFLAG',
            width: 100
        },
        {
            text: "高警訊藥品",
            dataIndex: 'DANGERDRUGFLAG',
            width: 100
        },
        {
            text: "高警訊藥品提示",
            dataIndex: 'DANGERDRUGMEMO',
            width: 100
        },
        {
            text: "冷藏存放",
            dataIndex: 'COLDSTORAGEFLAG',
            width: 100
        },
        {
            text: "避光存放",
            dataIndex: 'LIGHTAVOIDFLAG',
            width: 100
        },
        {
            text: "異動狀態",
            dataIndex: 'CHANGESTATUS',
            width: 100
        },
        {
            text: "門診給藥頻率",
            dataIndex: 'FREQNOO',
            width: 100
        },
        {
            text: "住院給藥頻率",
            dataIndex: 'FREQNOI',
            width: 100
        },
        {
            text: "預設開立天數",
            dataIndex: 'ORDERDAYS',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "劑量",
            dataIndex: 'DOSE',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "院內費用類別",
            dataIndex: 'HOSPCHARGEID1',
            width: 100
        },
        {
            text: "院內費用",
            dataIndex: 'HOSPCHARGEID2',
            width: 100
        },
        {
            text: "健保費用類別",
            dataIndex: 'INSUCHARGEID1',
            width: 100
        },
        {
            text: "健保費用",
            dataIndex: 'INSUCHARGEID2',
            width: 100
        },
        {
            text: "醫令類別",
            dataIndex: 'ORDERTYPE',
            width: 100
        },
        {
            text: "醫令類別(申報定義)",
            dataIndex: 'ORDERKIND',
            width: 120
        },
        {
            text: "高價用藥",
            dataIndex: 'HIGHPRICEFLAG',
            width: 100
        },
        {
            text: "特殊治療種類",
            dataIndex: 'CURETYPE',
            width: 100
        },
        {
            text: "住院醫囑顯示",
            dataIndex: 'INPDISPLAYFLAG',
            width: 100
        },
        {
            text: "開立醫令即為報到",
            dataIndex: 'SOONCULLFLAG',
            width: 120
        },
        {
            text: "替代院內代碼1",
            dataIndex: 'SUBSTITUTE1',
            width: 100
        },
        {
            text: "替代院內代碼2",
            dataIndex: 'SUBSTITUTE2',
            width: 100
        },
        {
            text: "替代院內代碼3",
            dataIndex: 'SUBSTITUTE3',
            width: 100
        },
        {
            text: "替代院內代碼4",
            dataIndex: 'SUBSTITUTE4',
            width: 100
        },
        {
            text: "替代院內代碼5",
            dataIndex: 'SUBSTITUTE5',
            width: 100
        },
        {
            text: "連帶否(門診)",
            dataIndex: 'RELATEFLAGO',
            width: 100
        },
        {
            text: "連帶否(住院)",
            dataIndex: 'RELATEFLAGI',
            width: 100
        },
        {
            text: "體重及安全量：計算別",
            dataIndex: 'WEIGHTTYPE',
            width: 130
        },
        {
            text: "體重及安全量：限制數量",
            dataIndex: 'WEIGHTUNITLIMIT',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 150
        },
        {
            text: "限制狀態",
            dataIndex: 'RESTRICTTYPE',
            width: 100
        },
        {
            text: "門診限制開立數量",
            dataIndex: 'MAXQTYO',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 120
        },
        {
            text: "住院限制開立數量",
            dataIndex: 'MAXQTYI',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 120
        },
        {
            text: "門診限制開立日數",
            dataIndex: 'MAXDAYSO',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 120
        },
        {
            text: "住院限制開立日數",
            dataIndex: 'MAXDAYSI',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 120
        },
        {
            text: "門診效期日數",
            dataIndex: 'VALIDDAYSO',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "住院效期日數",
            dataIndex: 'VALIDDAYSI',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "是否需報到",
            dataIndex: 'CHECKINSWITCH',
            width: 100
        },
        {
            text: "是否發報告",
            dataIndex: 'REPORTFLAG',
            width: 100
        },
        {
            text: "Single Item",
            dataIndex: 'SINGLEITEMFLAG',
            width: 100
        },
        {
            text: "手術碼",
            dataIndex: 'OPERATIONFLAG',
            width: 100
        },
        {
            text: "檢驗用藥",
            dataIndex: 'EXAMINEDRUGFLAG',
            width: 100
        },
        {
            text: "特定治療項目",
            dataIndex: 'MAINCUREITEM',
            width: 100
        },
        {
            text: "放射部位",
            dataIndex: 'RAYPOSITION',
            width: 100
        },
        {
            text: "空針代碼",
            dataIndex: 'CCORDERCODE',
            width: 100
        },
        {
            text: "放射第二張代碼",
            dataIndex: 'XRAYORDERCODE',
            width: 100
        },
        {
            text: "手術診斷碼",
            dataIndex: 'ORDIAGCODE',
            width: 100
        },
        {
            text: "醫令排序",
            dataIndex: 'ORDERCODESORT',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "傳送單位否",
            dataIndex: 'SENDUNITFLAG',
            width: 100
        },
        {
            text: "是否需 Sign In",
            dataIndex: 'SIGNFLAG',
            width: 100
        },
        {
            text: "除外項目",
            dataIndex: 'EXCLUDEFLAG',
            width: 100
        },
        {
            text: "處置需報調劑方式",
            dataIndex: 'NEEDOPDTYPEFLAG',
            width: 120
        },
        {
            text: "藥品成份Element1",
            dataIndex: 'DRUGELEMCODE1',
            width: 120
        },
        {
            text: "藥品成份Element2",
            dataIndex: 'DRUGELEMCODE2',
            width: 120
        },
        {
            text: "藥品成份Element3",
            dataIndex: 'DRUGELEMCODE3',
            width: 120
        },
        {
            text: "藥品成份Element4",
            dataIndex: 'DRUGELEMCODE4',
            width: 120
        },
        {
            text: "可線上異動否",
            dataIndex: 'CHANGEABLEFLAG',
            width: 100
        },
        {
            text: "TDM 藥品",
            dataIndex: 'TDMFLAG',
            width: 100
        },
        {
            text: "專案碼",
            dataIndex: 'SPECIALORDERKIND',
            width: 100
        },
        {
            text: "處置需報部位",
            dataIndex: 'NEEDREGIONFLAG',
            width: 100
        },
        {
            text: "醫令使用狀態",
            dataIndex: 'ORDERUSETYPE',
            width: 100
        },
        {
            text: "預設劑量",
            dataIndex: 'FIXDOSEFLAG',
            width: 100
        },
        {
            text: "罕見疾病用藥",
            dataIndex: 'RAREDISORDERFLAG',
            width: 100
        },
        {
            text: "內審用藥",
            dataIndex: 'HOSPEXAMINEFLAG',
            width: 100
        },
        {
            text: "給付條文代碼",
            dataIndex: 'ORDERCONDCODE',
            width: 100
        },
        {
            text: "內審限制用量",
            dataIndex: 'HOSPEXAMINEQTYFLAG',
            width: 100
        },
        {
            text: "記錄建立日期/時間",
            dataIndex: 'CREATEDATETIME',
            width: 120
        },
        {
            text: "記錄建立人員",
            dataIndex: 'CREATEOPID',
            width: 100
        },
        {
            text: "記錄處理日期/時間",
            dataIndex: 'PROCDATETIME',
            width: 120
        },
        {
            text: "記錄處理人員",
            dataIndex: 'PROCOPID',
            width: 100
        },
        {
            text: "BC肝用藥註記",
            dataIndex: 'HEPATITISCODE',
            width: 100
        },
        //{
        //    text: "院內代碼",
        //    dataIndex: 'OrderCode',
        //    width: 100
        //},
        {
            text: "生效起日",
            dataIndex: 'BEGINDATE',
            width: 100
        },
        {
            text: "生效迄日",
            dataIndex: 'ENDDATE',
            width: 100
        },
        {
            text: "門診扣庫轉換量",
            dataIndex: 'STOCKTRANSQTYO',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "住院扣庫轉換量",
            dataIndex: 'STOCKTRANSQTYI',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "門診劑型數量",
            dataIndex: 'ATTACHTRANSQTYO',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "住院劑型數量",
            dataIndex: 'ATTACHTRANSQTYI',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "健保點數一",
            dataIndex: 'INSUAMOUNT1',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "健保點數二",
            dataIndex: 'INSUAMOUNT2',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "自費點數一",
            dataIndex: 'PAYAMOUNT1',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "自費點數二",
            dataIndex: 'PAYAMOUNT2',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "進價",
            dataIndex: 'COSTAMOUNT',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "是否收管理費",
            dataIndex: 'MAMAGEFLAG',
            width: 100
        },
        {
            text: "管理費%",
            dataIndex: 'MAMAGERATE',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "健保碼",
            dataIndex: 'INSUORDERCODE',
            width: 100
        },
        {
            text: "健保負擔碼(住院)",
            dataIndex: 'INSUSIGNI',
            width: 100
        },
        {
            text: "健保負擔碼(門診)",
            dataIndex: 'INSUSIGNO',
            width: 100
        },
        {
            text: "健保急件加成",
            dataIndex: 'INSUEMGFLAG',
            width: 100
        },
        {
            text: "自費急件加成",
            dataIndex: 'HOSPEMGFLAG',
            width: 100
        },
        {
            text: "牙科轉診加成",
            dataIndex: 'DENTALREFFLAG',
            width: 100
        },
        {
            text: "提成類別碼",
            dataIndex: 'PPFTYPE',
            width: 100
        },
        {
            text: "提成百分比",
            dataIndex: 'PPFPERCENTAGE',
            width: 100
        },
        {
            text: "兒童加成否",
            dataIndex: 'INSUKIDFLAG',
            width: 100
        },
        {
            text: "自費兒童加成",
            dataIndex: 'HOSPKIDFLAG',
            width: 100
        },
        {
            text: "合約單價",
            dataIndex: 'CONTRACTPRICE',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "合約碼",
            dataIndex: 'CONTRACNO',
            width: 100
        },
        {
            text: "廠商代碼(供應商代碼)",
            dataIndex: 'SUPPLYNO',
            width: 120
        },
        {
            text: "標案來源",
            dataIndex: 'CASEFROM',
            width: 100
        },
        {
            text: "檢查折扣否",
            dataIndex: 'EXAMINEDISCFLAG',
            width: 100
        },
        {
            text: "製造廠名稱",
            dataIndex: 'ORIGINALPRODUCER',
            width: 100
        },
        {
            text: "申請商名稱",
            dataIndex: 'AGENTNAME',
            width: 100
        },
        //{
        //    text: "記錄建立日期/時間",
        //    dataIndex: 'CreateDateTime',
        //    width: 100
        //},
        //{
        //    text: "記錄建立人員",
        //    dataIndex: 'CreateOpID',
        //    width: 100
        //},
        //{
        //    text: "記錄處理日期/時間",
        //    dataIndex: 'ProcDateTime',
        //    width: 100
        //},
        //{
        //    text: "記錄處理人員",
        //    dataIndex: 'ProcOpID',
        //    width: 100
        //},
        {
            text: "結束日期時間",
            dataIndex: 'ENDDATETIME',
            width: 100
        },
        {
            header: "",
            flex: 1
        }],
        viewConfig: {
            forceFit: true,
            listeners: {
                //    refresh: function (dataview) {
                //         Ext.each(dataview.panel.columns, function (column) {
                //            column.autoSize()
                //        })
                //    }
            }
        },
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        TATabs.setActiveTab('Form');
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                if (T1LastRec) {
                    msglabel("");

                }
            }
        }
    });

    //點選master的項目後
    function setFormT1a() {
        //T1Grid.down('#export').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            //f.findField('x').setValue('U');
            //var u = f.findField('MAT_CLASS');
            //u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');

            TATabs.setActiveTab('Form');
        }
        else {
            T1Form.getForm().reset();
        }
    }


    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [{
            itemId: 'Query',
            title: '查詢',
            items: [T1QueryForm]
        }, {
            itemId: 'Form',
            title: '瀏覽',
            items: [T1Form]
        }]
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [{
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '100%',
                        items: [T1Grid]
                    },
                ]
            }]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [TATabs]
        }
        ]
    });


    function T1Load() {
        T1Tool.moveFirst();
    }


    SetOrderDCFlag();
    SetCaseFrom();
    SetInsuSignI();
    SetInsuSignO();
    T1Grid.down('#export').setDisabled(true);
});
