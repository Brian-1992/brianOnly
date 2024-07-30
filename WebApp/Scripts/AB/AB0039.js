Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Exp = '/api/AB0039/Excel';
    //var T1Name = "大批異動修改作業";
    var T1Name = '';

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_ordercode = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0039/GetOrdercodeCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_agenno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0039/GetAgennoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_insuordercode = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0039/GetInsuOrderCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_casefrom = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0039/GetCaseFromCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_sourcecode = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0039/GetSourceCodeCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 210;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '院內碼',
                    name: 'P0',
                    id: 'P0',
                    store: st_ordercode,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combo',
                    fieldLabel: '～',
                    name: 'P1',
                    id: 'P1',
                    store: st_ordercode,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    labelAlign: 'right',
                    labelSeparator: '',
                    labelWidth: 10,
                    width: 160,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combo',
                    fieldLabel: '健保碼',
                    name: 'P2',
                    id: 'P2',
                    store: st_insuordercode,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    anyMatch: true,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combo',
                    fieldLabel: '～',
                    name: 'P3',
                    id: 'P3',
                    store: st_insuordercode,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    labelAlign: 'right',
                    labelSeparator: '',
                    labelWidth: 10,
                    width: 160,
                    padding: '0 4 0 4'
                }
            ]
        }, {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '廠商代碼',
                        name: 'P4',
                        id: 'P4',
                        store: st_agenno,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                        labelAlign: 'right',
                        labelWidth: mLabelWidth,
                        width: mWidth,
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '～',
                        name: 'P5',
                        id: 'P5',
                        store: st_agenno,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                        labelAlign: 'right',
                        labelSeparator: '',
                        labelWidth: 10,
                        width: 160,
                        padding: '0 4 0 4'
                    }, 
                     {
                        xtype: 'combo',
                        fieldLabel: '標案來源',
                        name: 'P6',
                        id: 'P6',
                         store: st_casefrom,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                        labelAlign: 'right',
                        labelWidth: mLabelWidth,
                        width: mWidth,
                        anyMatch: true,
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '來源代碼',
                        name: 'P7',
                        id: 'P7',
                         store: st_sourcecode,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        labelAlign: 'right',
                        labelWidth: mLabelWidth,
                        width: mWidth,
                        padding: '0 4 0 4'
                    }
                ]
            },{
            xtype: 'panel',
            id: 'PanelP3',
            border: false,
            layout: 'hbox',
                items: [
                    {
                        xtype: 'datefield',
                        fieldLabel: '異動日期',
                        name: 'P8',
                        id: 'P8',
                        vtype: 'dateRange',
                        dateRange: { end: 'P9' },
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '～',
                        name: 'P9',
                        id: 'P9',
                        labelSeparator: '',
                        labelWidth: 10,
                        width: 160,
                        vtype: 'dateRange',
                        dateRange: { begin: 'P8' },
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            T1Load();
                            msglabel('訊息區:');
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            msglabel('訊息區:');
                        }
                    }, {
                        xtype: 'button',
                        text: '匯出',
                        id: 'btnExport',
                        handler: function () {
                            var f = this.up('form').getForm();
                            Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                                if (btn === 'yes') {
                                    var p = new Array();
                                    p.push({ name: 'FN', value: '大批異動修改作業.xls' }); //檔名
                                    p.push({ name: 'p0', value: f.findField('P0').getValue() }); //SQL篩選條件
                                    p.push({ name: 'p1', value: f.findField('P1').getValue() }); //SQL篩選條件
                                    p.push({ name: 'p2', value: f.findField('P2').getValue() }); //SQL篩選條件
                                    p.push({ name: 'p3', value: f.findField('P3').getValue() }); //SQL篩選條件
                                    p.push({ name: 'p4', value: f.findField('P4').getValue() }); //SQL篩選條件
                                    p.push({ name: 'p5', value: f.findField('P5').getValue() }); //SQL篩選條件
                                    p.push({ name: 'p6', value: f.findField('P6').getValue() }); //SQL篩選條件
                                    p.push({ name: 'p7', value: f.findField('P7').getValue() }); //SQL篩選條件
                                    p.push({ name: 'p8', value: f.findField('P8').getValue() }); //SQL篩選條件
                                    p.push({ name: 'p9', value: f.findField('P9').getValue() }); //SQL篩選條件
                                    WEBAPP.utils.Common.postForm(T1Exp, p);
                                }
                            });
                        }
                    }
                ]
            }
        ]
    });
    var T1Export = Ext.widget({
        xtype: 'form',
        defaultType: 'textfield',
        standardSubmit: true,
        url: T1Exp,
        items: [{
            name: 'sort'
        }, {
            name: 'p0'
        }, {
            name: 'p1'
        }, {
            name: 'p2'
        }, {
            name: 'p3'
        }, {
            name: 'p4'
        }, {
            name: 'p5'
        }, {
            name: 'p6'
        }, {
            name: 'p7'
        }, {
            name: 'p8'
        }, {
            name: 'p9'
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.AB0039', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                    p8: T1Query.getForm().findField('P8').rawValue,
                    p9: T1Query.getForm().findField('P9').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {
                Ext.getCmp('btnExport').setDisabled(store.getCount() == 0);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
    }
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
            layout: 'fit',
            items: [T1Query]
        }
        ],

        columns: [{
            xtype: 'rownumberer'
            }, {
                text: "院內代碼",
                dataIndex: 'ORDERCODE',
                width: 100
            }, {
                text: "健保碼",
                dataIndex: 'INSUORDERCODE',
                width: 100
            }, {
                text: "英文名稱",
                dataIndex: 'ORDERENGNAME',
                width: 100
            }, {
                text: "成份名稱",
                dataIndex: 'SCIENTIFICNAME',
                width: 100
            }, {
                text: "院內名稱",
                dataIndex: 'ORDERHOSPNAME',
                width: 100
            }, {
                text: "中文名稱",
                dataIndex: 'ORDERCHINNAME',
                width: 100
            }, {
                text: "簡稱(hp專用)",
                dataIndex: 'ORDEREASYNAME',
                width: 100
            }, {
                text: "藥委會品項",
                dataIndex: 'COMMITTEECODE',
                width: 100
            }, {
                text: "藥品性質欄位",
                dataIndex: 'DRUGCLASSIFY',
                width: 100
            }, {
                text: "醫令使用狀態",
                dataIndex: 'ORDERUSETYPE',
                width: 100
            }, {
                text: "給付條文代碼",
                dataIndex: 'ORDERCONDCODE',
                width: 100
            }, {
                text: "衛生署核准字號",
                dataIndex: 'DOHLICENSENO',
                width: 100
            }, {
                text: "藥品單複方",
                dataIndex: 'MULTIPRESCRIPTIONCODE',
                width: 100
            }, {
                text: "藥品成份1",
                dataIndex: 'DRUGELEMCODE1',
                width: 100
            }, {
                text: "成份量及單位",
                dataIndex: 'COMPONENTNUNIT',
                width: 100
            }, {
                text: "藥品成份2",
                dataIndex: 'DRUGELEMCODE2',
                width: 100
            }, {
                text: "成份量及單位2",
                dataIndex: 'COMPONENTNUNIT2',
                width: 100
            }, {
                text: "藥品成份3",
                dataIndex: 'DRUGELEMCODE3',
                width: 100
            }, {
                text: "藥品成份4",
                dataIndex: 'DRUGELEMCODE4',
                width: 100
            }, {
                text: "成份量及單位4",
                dataIndex: 'COMPONENTNUNIT4',
                width: 100
            }, {
                text: "醫囑單位",
                dataIndex: 'ORDERUNIT',
                width: 100
            }, {
                text: "中文單位",
                dataIndex: 'ORDERCHINUNIT',
                width: 100
            }, {
                text: "廠商代碼(供應商代碼)",
                dataIndex: 'SUPPLYNO',
                width: 100
            }, {
                text: "合約碼",
                dataIndex: 'CONTRACNO',
                width: 100
            }, {
                text: "標案來源",
                dataIndex: 'CASEFROM',
                width: 100
            }, {
                text: "健保價",
                dataIndex: 'INSUAMOUNT1',
                width: 100
            }, {
                text: "自費價",
                dataIndex: 'PAYAMOUNT1',
                width: 100
            }, {
                text: "買斷藥",
                dataIndex: 'BUYORDERFLAG',
                width: 100
            }, {
                text: "住院消耗歸整",
                dataIndex: 'CARRYKINDI',
                width: 100
            }, {
                text: "門急消耗歸整",
                dataIndex: 'CARRYKINDO',
                width: 100
            }, {
                text: "累計用藥",
                dataIndex: 'AGGREGATECODE',
                width: 100
            }, {
                text: "製造廠名稱",
                dataIndex: 'ORIGINALPRODUCER',
                width: 100
            }, {
                text: "申請商名稱",
                dataIndex: 'AGENTNAME',
                width: 100
            }, {
                text: "規格量及單位",
                dataIndex: 'SPECNUNIT',
                width: 100
            }, {
                text: "申報(計價)單位",
                dataIndex: 'ATTACHUNIT',
                width: 100
            }, {
                text: "扣庫單位",
                dataIndex: 'STOCKUNIT',
                width: 100
            }, {
                text: "UD服務",
                dataIndex: 'UDSERVICEFLAG',
                width: 100
            }, {
                text: "門診倍數核發",
                dataIndex: 'LIMITEDQTYO',
                width: 100
            }, {
                text: "住院倍數核發",
                dataIndex: 'LIMITEDQTYI',
                width: 100
            }, {
                text: "預設給藥途徑",
                dataIndex: 'PATHNO',
                width: 100
            }, {
                text: "公藥分類(三總藥學專用欄位)",
                dataIndex: 'PUBLICDRUGCODE',
                width: 100
            }, {
                text: "藥品類別",
                dataIndex: 'DRUGCLASS',
                width: 100
            }, {
                text: "研究用藥",
                dataIndex: 'RESEARCHDRUGFLAG',
                width: 100
            }, {
                text: "開立限制",
                dataIndex: 'LIMITFLAG',
                width: 100
            }, {
                text: "內審限制用量",
                dataIndex: 'HOSPEXAMINEQTYFLAG',
                width: 100
            }, {
                text: "限制狀態",
                dataIndex: 'RESTRICTTYPE',
                width: 100
            }, {
                text: "限制次數",
                dataIndex: 'MAXTAKETIMES',
                width: 100
            }, {
                text: "門診限制開立數量",
                dataIndex: 'MAXQTYO',
                width: 100
            }, {
                text: "門診限制開立日數",
                dataIndex: 'MAXDAYSO',
                width: 100
            }, {
                text: "門診效期日數",
                dataIndex: 'VALIDDAYSO',
                width: 100
            }, {
                text: "住院限制開立數量",
                dataIndex: 'MAXQTYI',
                width: 100
            }, {
                text: "住院限制開立日數",
                dataIndex: 'MAXDAYSI',
                width: 100
            }, {
                text: "住院效期日數",
                dataIndex: 'VALIDDAYSI',
                width: 100
            }, {
                text: "限制途徑",
                dataIndex: 'FIXPATHNOFLAG',
                width: 100
            }, {
                text: "服用藥別",
                dataIndex: 'TAKEKIND',
                width: 100
            }, {
                text: "抗生素等級",
                dataIndex: 'ANTIBIOTICSCODE',
                width: 100
            }, {
                text: "管制用藥",
                dataIndex: 'RESTRICTCODE',
                width: 100
            }, {
                text: "住院給藥頻率",
                dataIndex: 'FREQNOI',
                width: 100
            }, {
                text: "門診給藥頻率",
                dataIndex: 'FREQNOO',
                width: 100
            }, {
                text: "預設劑量",
                dataIndex: 'DOSE',
                width: 100
            }, {
                text: "預設劑量",
                dataIndex: 'DOSE1',
                width: 100
            }, {
                text: "藥品劑型",
                dataIndex: 'ORDERABLEDRUGFORM',
                width: 100
            }, {
                text: "罕見疾病用藥",
                dataIndex: 'RAREDISORDERFLAG',
                width: 100
            }, {
                text: "外審(健保專案)用藥",
                dataIndex: 'SPECIALORDERKIND ',
                width: 100
            }, {
                text: "內審用藥",
                dataIndex: 'HOSPEXAMINEFLAG',
                width: 100
            }, {
                text: "一次極量",
                dataIndex: 'MAXQTYPERTIME',
                width: 100
            }, {
                text: "一日極量",
                dataIndex: 'MAXQTYPERDAY',
                width: 100
            }, {
                text: "不可剝半",
                dataIndex: 'ONLYROUNDFLAG',
                width: 100
            }, {
                text: "不可磨粉",
                dataIndex: 'UNABLEPOWDERFLAG',
                width: 100
            }, {
                text: "冷藏存放",
                dataIndex: 'COLDSTORAGEFLAG',
                width: 100
            }, {
                text: "避光存放",
                dataIndex: 'LIGHTAVOIDFLAG',
                width: 100
            }, {
                text: "體重及安全量：計算別",
                dataIndex: 'WEIGHTTYPE',
                width: 100
            }, {
                text: "體重及安全量：限制數量",
                dataIndex: 'WEIGHTTYPE',
                width: 100
            }, {
                text: "高警訊藥品",
                dataIndex: 'DANGERDRUGFLAG',
                width: 100
            }, {
                text: "高警訊藥品提示",
                dataIndex: 'DANGERDRUGMEMO',
                width: 100
            }, {
                text: "藥品外觀",
                dataIndex: 'DRUGEXTERIOR',
                width: 100
            }, {
                text: "藥品外觀(英文)",
                dataIndex: 'DRUGENGEXTERIOR',
                width: 100
            }, {
                text: "適應症(中文)",
                dataIndex: 'SYMPTOMCHIN',
                width: 100
            }, {
                text: "適應症(英文)",
                dataIndex: 'SYMPTOMENG',
                width: 100
            }, {
                text: "主要副作用(中文)",
                dataIndex: 'CHINSIDEEFFECT',
                width: 100
            }, {
                text: "主要副作用(英文)",
                dataIndex: 'ENGSIDEEFFECT',
                width: 100
            }, {
                text: "注意事項(中文)",
                dataIndex: 'CHINATTENTION',
                width: 100
            }, {
                text: "注意事項(英文)",
                dataIndex: 'ENGATTENTION',
                width: 100
            }, {
                text: "衛生署核准適應症",
                dataIndex: 'DOHLICENSENO',
                width: 100
            }, {
                text: "FDA核准適應症",
                dataIndex: 'FDASYMPTOM',
                width: 100
            }, {
                text: "處方集",
                dataIndex: 'DRUGMEMO',
                width: 100
            }, {
                text: "授乳安全性",
                dataIndex: 'SUCKLESECURITY',
                width: 100
            }, {
                text: "懷孕分級",
                dataIndex: 'PREGNANTGRADE',
                width: 100
            }, {
                text: "藥品圖片檔名",
                dataIndex: 'DRUGPICTURELINK',
                width: 100
            }, {
                text: "藥品仿單檔名",
                dataIndex: 'DRUGLEAFLETLINK',
                width: 100
            }, {
                text: "TDM 藥品",
                dataIndex: 'TDMFLAG',
                width: 100
            }, {
                text: "TDM 合理治療濃度上限",
                dataIndex: 'TDMFLAG1',
                width: 100
            }, {
                text: "TDM 合理治療濃度下限",
                dataIndex: 'TDMFLAG2',
                width: 100
            }, {
                text: "TDM 合理PEAK起",
                dataIndex: 'TDMFLAG3',
                width: 100
            }, {
                text: "TDM 合理PEAK迄",
                dataIndex: 'TDMFLAG4',
                width: 100
            }, {
                text: "TDM 合理 Trough 起",
                dataIndex: 'TDMFLAG5',
                width: 100
            }, {
                text: "TDM 合理 Trough 迄",
                dataIndex: 'TDMFLAG6',
                width: 100
            }, {
                text: "TDM 危急值 起",
                dataIndex: 'TDMFLAG7',
                width: 100
            }, {
                text: "TDM 危急值 迄",
                dataIndex: 'TDMFLAG8',
                width: 100
            }, {
                text: "TDM 備註1",
                dataIndex: 'TDMFLAG9',
                width: 100
            }, {
                text: "TDM 備註2",
                dataIndex: 'TDMFLAG10',
                width: 100
            }, {
                text: "TDM 備註3",
                dataIndex: 'TDMFLAG11',
                width: 100
            }, {
                text: "UD磨粉",
                dataIndex: 'UDPOWDERFLAG',
                width: 100
            }, {
                text: "藥包機品項",
                dataIndex: 'MACHINEFLAG',
                width: 100
            }, {
                text: "成份母層代碼1",
                dataIndex: 'DRUGPARENTCODE1',
                width: 100
            }, {
                text: "成份母層代碼2",
                dataIndex: 'DRUGPARENTCODE2',
                width: 100
            }, {
                text: "成份母層代碼3",
                dataIndex: 'DRUGPARENTCODE3',
                width: 100
            }, {
                text: "成份母層代碼4",
                dataIndex: 'DRUGPARENTCODE4',
                width: 100
            }, {
                text: "藥品包裝",
                dataIndex: 'DRUGPACKAGE',
                width: 100
            }, {
                text: "藥理分類ATC1",
                dataIndex: 'ATCCODE1',
                width: 100
            }, {
                text: "藥理分類AHFS1",
                dataIndex: 'ATCCODE2',
                width: 100
            }, {
                text: "藥理分類ATC2",
                dataIndex: 'ATCCODE3',
                width: 100
            }, {
                text: "藥理分類AHFS2",
                dataIndex: 'ATCCODE4',
                width: 100
            }, {
                text: "藥理分類ATC3",
                dataIndex: 'ATCCODE5',
                width: 100
            }, {
                text: "藥理分類AHFS3",
                dataIndex: 'ATCCODE6',
                width: 100
            }, {
                text: "藥理分類ATC4",
                dataIndex: 'ATCCODE7',
                width: 100
            }, {
                text: "藥理分類AHFS4",
                dataIndex: 'ATCCODE8',
                width: 100
            }, {
                text: "老年人劑量調整",
                dataIndex: 'GERIATRIC',
                width: 100
            }, {
                text: "肝功能不良需調整劑量",
                dataIndex: 'LIVERLIMITED',
                width: 100
            }, {
                text: "腎功能不良需調整劑量",
                dataIndex: 'RENALLIMITED',
                width: 100
            }, {
                text: "生物製劑",
                dataIndex: 'BIOLOGICALAGENT',
                width: 100
            }, {
                text: "血液製劑",
                dataIndex: 'BLOODPRODUCT',
                width: 100
            }, {
                text: "是否需冷凍",
                dataIndex: 'FREEZING',
                width: 100
            }, {
                text: "CDC藥品",
                dataIndex: 'RETURNDRUGFLAG',
                width: 100
            }, {
                sortable: false,
                flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            }
        }
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
            items: [T1Grid]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();
});
