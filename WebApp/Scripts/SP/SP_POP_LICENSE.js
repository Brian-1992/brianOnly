Ext.Loader.loadScript({ url: location.pathname.substring(0, location.pathname.indexOf('Form')) + 'Scripts/SP/SP01_detail_store.js' });
Ext.onReady(function () {
    function T1Load() {
        GetFileList('PSL');
        GetFileList('PML');
        GetFileList('BRC');
        GetFileList('FRC');

        if (edit == 0) {
            Ext.getCmp('PSL_FILE').setVisible(false);
            Ext.getCmp('PML_FILE').setVisible(false);
            Ext.getCmp('BRC_FILE').setVisible(false);
            Ext.getCmp('FRC_FILE').setVisible(false);
        }
    }

    var T1Form = Ext.widget({
        xtype: 'form',
        id: 'POP_MODIFY',
        bodyStyle: 'margin:5px;border:none',
        //layout: {
        //    type: 'vbox'
        //    //tdAttrs: { valign: 'top' }
        //},
        autoScroll: true,
        //layout: 'auto',
        items: [
            {
                xtype: 'hiddenfield',
                id: 'RLNO',
                name: 'RLNO',
                value: ''
            },
            {
                xtype: 'fieldset',
                //columnWidth: 0.5,
                width: '100%',
                //autoScroll: true,
                //style: "margin:5px;margin-top:10px;background-color: #d2e9ff;",
                layout: {
                    type: 'vbox',
                    //columns: 1
                },
                items: [
                    {
                        xtype: 'container',
                        //width: 800,
                        layout: {
                            type: 'hbox',
                        },
                        items: [
                            {
                                xtype: 'container',
                                layout: 'hbox',
                                padding: '0 0 4 0',
                                width: 450,
                                items: [
                                    {
                                        xtype: 'displayfield',
                                        fieldLabel: '販售業藥商許可執照字號',
                                        id: 'PSL',
                                        name: 'PSL',
                                        width: 400,
                                        labelWidth: 200,
                                        labelAlign: 'right'
                                    }, {
                                        xtype: 'button',
                                        text: '檔案',
                                        id: 'PSL_FILE',
                                        width: 45,
                                        handler: function () {
                                            showUploadWindow(true, true, 'SP_POP_LICENSE,' + code + ',PSL', '上傳檔案[販售業藥商許可執照字號]', viewport, { closeCallback: function () { GetFileList('PSL') } });
                                        }
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        xtype: 'container',
                        //width: 800,
                        layout: {
                            type: 'hbox',
                        },
                        items: [
                            {
                                xtype: 'container',
                                layout: 'hbox',
                                padding: '0 0 4 0',
                                width: 450,
                                items: [
                                    {
                                        xtype: 'displayfield',
                                        fieldLabel: '製藥業藥商許可執照字號',
                                        id: 'PML',
                                        name: 'PML',
                                        width: 400,
                                        labelWidth: 200,
                                        labelAlign: 'right'
                                    }, {
                                        xtype: 'button',
                                        text: '檔案',
                                        id: 'PML_FILE',
                                        width: 45,
                                        handler: function () {
                                            showUploadWindow(true, true, 'SP_POP_LICENSE,' + code + ',PML', '上傳檔案[製藥業藥商許可執照字號]', viewport, { closeCallback: function () { GetFileList('PML') } });
                                        }
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        xtype: 'container',
                        //width: 800,
                        layout: {
                            type: 'hbox',
                        },
                        items: [
                            {
                                xtype: 'container',
                                layout: 'hbox',
                                padding: '0 0 4 0',
                                width: 450,
                                items: [
                                    {
                                        xtype: 'displayfield',
                                        fieldLabel: '營利事業登記證',
                                        id: 'BRC',
                                        name: 'BRC',
                                        width: 400,
                                        labelWidth: 200,
                                        labelAlign: 'right'
                                    }, {
                                        xtype: 'button',
                                        text: '檔案',
                                        id: 'BRC_FILE',
                                        width: 45,
                                        handler: function () {
                                            showUploadWindow(true, true, 'SP_POP_LICENSE,' + code + ',BRC', '上傳檔案[營利事業登記證]', viewport, { closeCallback: function () { GetFileList('BRC') } });
                                        }
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        xtype: 'container',
                        //width: 800,
                        layout: {
                            type: 'hbox',
                        },
                        items: [
                            {
                                xtype: 'container',
                                layout: 'hbox',
                                padding: '0 0 4 0',
                                width: 450,
                                items: [
                                    {
                                        xtype: 'displayfield',
                                        fieldLabel: '工廠登記證',
                                        id: 'FRC',
                                        name: 'FRC',
                                        width: 400,
                                        labelWidth: 200,
                                        labelAlign: 'right'
                                    }, {
                                        xtype: 'button',
                                        text: '檔案',
                                        id: 'FRC_FILE',
                                        width: 45,
                                        handler: function () {
                                            showUploadWindow(true, true, 'SP_POP_LICENSE,' + code + ',FRC', '上傳檔案[工廠登記證]', viewport, { closeCallback: function () { GetFileList('FRC') } });
                                        }
                                    }
                                ]
                            }
                        ]
                    },
                ]
            }
        ]
    });

    function GetFileList(obj_id) {
        var rtn = '';
        Ext.Ajax.request({
            url: '../../../api/SP/GetFileList1',
            method: reqVal_p,
            params: {
                p0: 'SP_POP_LICENSE',
                p1: code,
                p2: obj_id
            },
            async: false,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    for (var i = 0; i < data.etts.length; i++) {
                        rtn += data.etts[i].FD + '&nbsp;' + '<a href="javascript:DownloadFile(\'' + data.etts[i].FG + '\')" >' + data.etts[i].FN + '</a> <br>';
                    }
                    Ext.getCmp(obj_id).setValue(rtn);
                }
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
    }

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
            //itemId: 'form',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Form]
        }]
    });

    T1Load();

});
